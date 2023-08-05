using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using TestsHelper.SourceGenerator.Attributes;
using TestsHelper.SourceGenerator.CodeBuilding;
using TestsHelper.SourceGenerator.CodeBuilding.Types;
using TestsHelper.SourceGenerator.Diagnostics;
using TestsHelper.SourceGenerator.Diagnostics.Exceptions;
using TestsHelper.SourceGenerator.MockFilling.Models;
using TestsHelper.SourceGenerator.MockFilling.PartialImplementation;

namespace TestsHelper.SourceGenerator.MockFilling;

public class MockFillerImplementation
{
    private static readonly List<FileResult> EmptyResult = new List<FileResult>(0);

    public IReadOnlyList<FileResult> Generate(TestClassMockCandidate testClassMockCandidate)
    {
        if (!TryExtractOneTestedClassMember(testClassMockCandidate, out AttributedTestClassMember member))
        {
            return EmptyResult;
        }

        ImmutableList<IMethodSymbol> constructors = member.Symbol
            .GetMembers()
            .Where(symbol => symbol.Kind == SymbolKind.Method)
            .OfType<IMethodSymbol>()
            .Where(methodSymbol => methodSymbol.MethodKind == MethodKind.Constructor)
            .ToImmutableList();

        // TODO: make this smarter
        IMethodSymbol selectedConstructor = constructors[0];

        Dictionary<string, IDependencyInitializationBehavior> dependencyBehaviors = new();

        // Default Values
        dependencyBehaviors.AddKeysIfNotExists(
            FindDefaultValueFields(testClassMockCandidate.ContainingClassSymbol, selectedConstructor),
            pair => pair.Key,
            pair => new PredefinedValueDependencyInitialization(pair.Value.Name)
        );

        // Add Mocks For Parameters With No Default Value
        dependencyBehaviors.AddKeysIfNotExists(
            selectedConstructor.Parameters,
            symbol => symbol.Name,
            symbol => new MockDependencyInitialization(symbol.Type)
        );

        List<FileBuilder> fileBuilders = PartialClassCreator.Create(
            dependencyBehaviors,
            testClassMockCandidate.ContainingClassIdentifier.Text, 
            testClassMockCandidate.ContainsClassNamespace, 
            member.GenerateMockWrapper ? WrapperGenerationMode.MethodsWrap : WrapperGenerationMode.OnlyMockWrap,
            selectedConstructor,
            selectedConstructor.ContainingType.Type()
        );

        return GetFilesResults(fileBuilders).ToList();
    }

    private static bool TryExtractOneTestedClassMember(TestClassMockCandidate candidate, out AttributedTestClassMember member)
    {
        switch (candidate.AttributedTestClassMembers.Length)
        {
            case 0:
                member = default;
                return false;
            case 1:
                member = candidate.AttributedTestClassMembers[0];
                return true;
            default:
                throw new DiagnosticException(DiagnosticRegistry.MoreThanOneFillMockUsage,candidate.ContainingClassIdentifier.GetLocation());
        }
    }

    [Pure]
    private static IEnumerable<FileResult> GetFilesResults(IEnumerable<FileBuilder> fileBuilders)
    {
        foreach (FileBuilder fileBuilder in fileBuilders.OrderBy(builder => builder.Name))
        {
            string code = fileBuilder.Build().NormalizeWhitespace(eol:Environment.NewLine).ToFullString();

            yield return new FileResult(fileBuilder.Name, SourceText.From(code, Encoding.UTF8));
        }
    }

    private static ImmutableDictionary<string, IFieldSymbol> FindDefaultValueFields(INamedTypeSymbol declaration, IMethodSymbol constructor)
    {
        Dictionary<string, ITypeSymbol> parametersNameToType = constructor.Parameters
            .ToDictionary(parameter => parameter.Name, parameter => parameter.Type);

        Dictionary<string, IFieldSymbol> defaultValuesFields = new();

        List<Diagnostic> diagnostics = new();
        foreach (IFieldSymbol fieldSymbol in declaration.GetMembers().OfType<IFieldSymbol>())
        {
            AttributeData? attributeData = fieldSymbol.GetAttributes().FirstOrDefault(SameAttribute<DefaultValueAttribute>);
            if (attributeData == null) continue;

            TypedConstant fieldNameArgument = attributeData.ConstructorArguments[0];
            if (fieldNameArgument is not {IsNull: false, Value: not null})
                continue;

            string fieldName = (string) fieldNameArgument.Value;

            //TODO: Provide Location
            if (!parametersNameToType.ContainsKey(fieldName))
            {
                diagnostics.Add(Diagnostic.Create(DiagnosticRegistry.DefaultValueToUnknownParameter, Location.None, fieldName));
                continue;
            }

            if (!AreSymbolsEquals(parametersNameToType[fieldName], fieldSymbol.Type))
            {
                diagnostics.Add(Diagnostic.Create(
                    DiagnosticRegistry.DefaultValueWithWrongType,
                    Location.None,
                    fieldSymbol.Type.Name,
                    fieldName,
                    parametersNameToType[fieldName].Name
                ));
                continue;
            }

            defaultValuesFields[fieldName] = fieldSymbol;
        }

        if (diagnostics.Count > 0)
        {
            throw new MultipleDiagnosticsException(diagnostics);
        }

        return defaultValuesFields.ToImmutableDictionary();
    }

    private static bool SameAttribute<TAttribute>(AttributeData attributeData) where TAttribute : Attribute
    {
        return attributeData.AttributeClass!.ToDisplayString() == typeof(TAttribute).FullName;
    }

    private static bool AreSymbolsEquals(ISymbol first, ISymbol second)
    {
        return SymbolEqualityComparer.Default.Equals(first, second);
    }
}