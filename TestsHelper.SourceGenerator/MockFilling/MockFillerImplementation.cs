using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using TestsHelper.SourceGenerator.MockFilling.Models;
using TestsHelper.SourceGenerator.MockFilling.PartialImplementation;

namespace TestsHelper.SourceGenerator.MockFilling;

public class MockFillerImplementation
{
    public MockFillerOutput Generate(ClassToFillMockIn classToFillMockIn)
    {
        IMockedFilledPartialClassCreator partialImplementation = new SyntaxTreeMockedFilledPartialClassCreator();

        ImmutableList<IMethodSymbol> constructors = classToFillMockIn.TestedClassMember
            .GetMembers()
            .Where(symbol => symbol.Kind == SymbolKind.Method)
            .OfType<IMethodSymbol>()
            .Where(methodSymbol => methodSymbol.MethodKind == MethodKind.Constructor)
            .ToImmutableList();

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

        SourceText sourceCode = partialImplementation.Build();
        return new MockFillerOutput(
            FileName: $"{classToFillMockIn.DeclarationSyntax.Identifier.Text}.FilledMock.generated.cs",
            SourceCode: sourceCode
        );
    }

    private static ImmutableDictionary<string, IFieldSymbol> FindDefaultValueFields(INamedTypeSymbol declaration, IMethodSymbol constructor)
    {
        List<IFieldSymbol> fields = declaration.GetMembers().OfType<IFieldSymbol>().ToList();
        List<(string Name, ITypeSymbol Type)> parameters = constructor.Parameters
            .Select(parameter => (parameter.Name, parameter.Type))
            .ToList();

        Dictionary<string, IFieldSymbol> defaultValuesFields = new();
        
        foreach ((string Name, ITypeSymbol Type) parameter in parameters)
        {
            string defaultValueFieldName = $"DefaultValue{parameter.Name}";

            foreach (IFieldSymbol field in fields.Where(field => AreSymbolsEquals(field.Type, parameter.Type)))
            {
                string cleanedName = CleanVariableName(field.Name);
                if (cleanedName.Equals(defaultValueFieldName, StringComparison.OrdinalIgnoreCase))
                {
                    defaultValuesFields[parameter.Name] = field;
                }
            }
        }

        return defaultValuesFields.ToImmutableDictionary();
    }

    private static string CleanVariableName(string name) => Regex.Replace(name, "[_-]", string.Empty);

    private static bool AreSymbolsEquals(ISymbol first, ISymbol second)
    {
        return SymbolEqualityComparer.Default.Equals(first, second);
    }
}