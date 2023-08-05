using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using TestsHelper.SourceGenerator.CodeBuilding;
using TestsHelper.SourceGenerator.CodeBuilding.Types;
using TestsHelper.SourceGenerator.MockFilling.PartialImplementation.DependencyMethodWrapperGenerator;
using TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Types;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation.DependencyWrapperGenerators;

public class DependencyWrapperGenerator : IDependencyWrapperGenerator
{
    private readonly IDependencyMethodWrapperClassGenerator _dependencyMethodWrapperClassGenerator;

    public DependencyWrapperGenerator(IDependencyMethodWrapperClassGenerator dependencyMethodWrapperClassGenerator)
    {
        _dependencyMethodWrapperClassGenerator = dependencyMethodWrapperClassGenerator;
    }

    public void GenerateCode(TypeBuilder builder, ITypeSymbol dependencyType)
    {
        builder.Name = $"Wrapper_{dependencyType.Name}";
        builder.Public();

        //TODO: make this not coupled to moq
        PropertyBuilder mockField = PropertyBuilder.Create(Moq.Mock.Generic(dependencyType), "Mock", autoGetter: true)
            .Add(builder);
        mockField.Public();

        ConstructorBuilder constructorBuilder = ConstructorBuilder.CreateAndAdd(builder);
        constructorBuilder.Public();

        ParameterBuilder mockParameter = constructorBuilder.InitializeFieldWithParameter(mockField, "mock");
        ParameterBuilder converterParameter = ParameterBuilder.Create(CommonTypes.IValueConverter, "converter");
        constructorBuilder.AddParameters(converterParameter);

        // Create Method Wrappers
        CreateMethodWrappers(builder, dependencyType, constructorBuilder, mockParameter.Name, converterParameter.Name);
    }

    private void CreateMethodWrappers(TypeBuilder builder, ITypeSymbol dependencyType, ConstructorBuilder constructorBuilder,
        params string[] methodWrapperClassParameters)
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
            TypeBuilder methodWrapperClass = builder.AddClass();
            _dependencyMethodWrapperClassGenerator.Generate(methodWrapperClass, dependencyType.Type(), method);

            IType type = methodWrapperClass.Type();
            PropertyBuilder methodProperty = PropertyBuilder.Create(type, name, autoGetter: true).Add(builder)
                .Public();

            constructorBuilder.AddBodyStatement($"{methodProperty} = new {type}({methodWrapperClassParameters:,});");
        }
    }
}