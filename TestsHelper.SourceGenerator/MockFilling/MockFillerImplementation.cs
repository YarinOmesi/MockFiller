using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.IO;
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
    public IReadOnlyList<FileResult> Generate(ClassToFillMockIn classToFillMockIn)
    {
        ImmutableList<IMethodSymbol> constructors = classToFillMockIn.TestedClassMember
            .GetMembers()
            .Where(symbol => symbol.Kind == SymbolKind.Method)
            .OfType<IMethodSymbol>()
            .Where(methodSymbol => methodSymbol.MethodKind == MethodKind.Constructor)
            .ToImmutableList();


        // TODO: make this smarter
        IMethodSymbol selectedConstructor = constructors[0];

        Dictionary<string, IDependencyBehavior> dependencyBehaviors = new();

        // Default Values
        dependencyBehaviors.AddKeysIfNotExists(
            FindDefaultValueFields(classToFillMockIn.DeclarationSymbol, selectedConstructor),
            pair => pair.Key,
            pair => new PredefinedValueDependencyBehavior(pair.Value.Name)
        );

        // Add Mocks For Parameters With No Default Value
        dependencyBehaviors.AddKeysIfNotExists(
            selectedConstructor.Parameters,
            symbol => symbol.Name,
            symbol => new MockDependencyBehavior(symbol.Type)
        );

        List<FileBuilder> fileBuilders = StringPartialCreator.Create(
            dependencyBehaviors,
            classToFillMockIn.DeclarationSyntax,
            WrapperGenerationMode.MethodsWrap,
            selectedConstructor,
            selectedConstructor.ContainingType.Type()
        );

        return GetFilesResults(fileBuilders);
    }

    [Pure]
    private static IReadOnlyList<FileResult> GetFilesResults(IEnumerable<FileBuilder> fileBuilders)
    {
        List<FileResult> results = new List<FileResult>();

        foreach (FileBuilder fileBuilder in fileBuilders.OrderBy(builder => builder.Name))
        {
            var stringWriter = new StringWriter();
            var indentedStringWriter = new IndentedStringWriter(stringWriter, "    ");

            fileBuilder.Write(indentedStringWriter);

            results.Add(new FileResult(fileBuilder.Name, SourceText.From(stringWriter.ToString(), Encoding.UTF8)));
        }

        return results;
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