using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using TestsHelper.SourceGenerator.CodeBuilding;
using TestsHelper.SourceGenerator.CodeBuilding.Abstractions;
using TestsHelper.SourceGenerator.CodeBuilding.Types;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation;

public class DependencyWrapperGenerator
{
    private readonly DependencyMethodWrapperClassGenerator _dependencyMethodWrapperClassGenerator;

    public DependencyWrapperGenerator(DependencyMethodWrapperClassGenerator dependencyMethodWrapperClassGenerator)
    {
        _dependencyMethodWrapperClassGenerator = dependencyMethodWrapperClassGenerator;
    }

    public void GenerateCode(ITypeBuilder builder, ITypeSymbol dependencyType)
    {
        builder.Name = $"Wrapper_{dependencyType.Name}";
        builder.AddModifiers("public");

        //TODO: make this not coupled to moq
        IPropertyBuilder mockField = builder.AddProperty(Moq.Mock.Generic(dependencyType.Type()), "Mock");
        mockField.AddModifiers("public");
        mockField.AutoSetter = false;

        IConstructorBuilder constructorBuilder = builder.AddConstructor();
        constructorBuilder.AddModifiers("public");

        IParameterBuilder mockParameter = constructorBuilder.InitializeFieldWithParameter(mockField, "mock");
        IParameterBuilder converterParameter = constructorBuilder.AddParameter(CommonTypes.ConverterType, "converter");

        CreateMethodWrappers(builder, constructorBuilder, mockParameter, converterParameter, dependencyType);
    }

    private void CreateMethodWrappers(
        ITypeBuilder builder,
        IConstructorBuilder constructorBuilder,
        IParameterBuilder mockParameter,
        IParameterBuilder converterParameter,
        ITypeSymbol dependencyType)
    {
#pragma warning disable RS1024
        Dictionary<string, IReadOnlyList<IMethodSymbol>> publicMethodsByName = dependencyType.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(method => method.DeclaredAccessibility == Accessibility.Public)
            .GroupBy<IMethodSymbol, string>(symbol => symbol.Name)
            .ToDictionary(grouping => grouping.Key, grouping => (IReadOnlyList<IMethodSymbol>) grouping.ToList());
#pragma warning restore RS1024

        foreach ((string name, IReadOnlyList<IMethodSymbol> methods) in publicMethodsByName.Select(pair => (pair.Key, pair.Value)))
        {
            // Use The Longest Parameters Method
            IMethodSymbol method = methods.OrderByDescending(symbol => symbol.Parameters.Length).First();

            // Method_type
            ITypeBuilder methodWrapperClass = builder.AddClass();
            _dependencyMethodWrapperClassGenerator.CreateMethodWrapperClass(methodWrapperClass, dependencyType.Type(), method);

            IPropertyBuilder methodProperty = builder.AddProperty(methodWrapperClass.Type(), name);
            methodProperty.AddModifiers("public");
            methodProperty.AutoSetter = false;

            constructorBuilder.AddBodyStatements(
                $"{methodProperty.Name} = new {methodWrapperClass.Name}({mockParameter.Name}, {converterParameter.Name});"
            );
        }
    }
}