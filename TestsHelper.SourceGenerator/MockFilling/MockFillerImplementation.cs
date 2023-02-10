using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using TestsHelper.SourceGenerator.Attributes;
using TestsHelper.SourceGenerator.Diagnostics;
using TestsHelper.SourceGenerator.MockFilling.Models;
using TestsHelper.SourceGenerator.MockFilling.PartialImplementation;

namespace TestsHelper.SourceGenerator.MockFilling;

public class MockFillerImplementation
{
    public IReadOnlyList<FileResult> Generate(ClassToFillMockIn classToFillMockIn)
    {
        IMockedFilledPartialClassCreator partialImplementation = new SyntaxTreeMockedFilledPartialClassCreator();

        ImmutableList<IMethodSymbol> constructors = classToFillMockIn.TestedClassMember
            .GetMembers()
            .Where(symbol => symbol.Kind == SymbolKind.Method)
            .OfType<IMethodSymbol>()
            .Where(methodSymbol => methodSymbol.MethodKind == MethodKind.Constructor)
            .ToImmutableList();

        partialImplementation.SetGenerateMockWrapper(classToFillMockIn.GenerateMockWrappers);

        // TODO: make this smarter
        IMethodSymbol selectedConstructor = constructors[0];
        partialImplementation.SetClassInfo(classToFillMockIn.DeclarationSyntax, selectedConstructor);

        ImmutableDictionary<string, IFieldSymbol> defaultValueFields =
            FindDefaultValueFields(classToFillMockIn.DeclarationSymbol, selectedConstructor);


        foreach (KeyValuePair<string, IFieldSymbol> defaultValueField in defaultValueFields)
        {
            string parameterName = defaultValueField.Key;
            IFieldSymbol field = defaultValueField.Value;
            partialImplementation.AddValueForParameter(field.Name, parameterName);
        }

        // Add Mocks For Parameters With No Default Value
        foreach (IParameterSymbol parameterSymbol in selectedConstructor.Parameters)
        {
            if (!defaultValueFields.ContainsKey(parameterSymbol.Name))
            {
                partialImplementation.AddMockForType(parameterSymbol.Type, parameterSymbol.Name);
            }
        }

        return partialImplementation.Build();
    }

    private static ImmutableDictionary<string, IFieldSymbol> FindDefaultValueFields(INamedTypeSymbol declaration, IMethodSymbol constructor)
    {
        Dictionary<string, ITypeSymbol> parameters = constructor.Parameters
            .ToDictionary(parameter => parameter.Name, parameter => parameter.Type);

        Dictionary<string, IFieldSymbol> defaultValuesFields = new();

        foreach (IFieldSymbol fieldSymbol in declaration.GetMembers().OfType<IFieldSymbol>())
        {
            AttributeData? attributeData = fieldSymbol.GetAttributes().FirstOrDefault(SameAttribute<DefaultValueAttribute>);
            if (attributeData == null) continue;

            TypedConstant fieldNameArgument = attributeData.ConstructorArguments[0];
            if (fieldNameArgument is not {Kind: TypedConstantKind.Primitive, IsNull: false, Value: not null})
                continue;

            string fieldName = (string) fieldNameArgument.Value;

            //TODO: Provide Location
            if (!parameters.ContainsKey(fieldName))
            {
                GlobalDiagnosticReporter.Report(DiagnosticRegistry.DefaultValueToUnknownParameter, Location.None, fieldName);
                continue;
            }

            if (!AreSymbolsEquals(parameters[fieldName], fieldSymbol.Type))
            {
                GlobalDiagnosticReporter.Report(
                    DiagnosticRegistry.DefaultValueWithWrongType,
                    Location.None,
                    fieldSymbol.Type.Name,
                    fieldName,
                    parameters[fieldName].Name
                );
                continue;
            }

            defaultValuesFields[fieldName] = fieldSymbol;
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