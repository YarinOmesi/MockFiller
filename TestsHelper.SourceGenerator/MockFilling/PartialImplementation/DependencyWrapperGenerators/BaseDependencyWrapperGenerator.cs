using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using TestsHelper.SourceGenerator.CodeBuilding;
using TestsHelper.SourceGenerator.CodeBuilding.Types;
using TestsHelper.SourceGenerator.MockFilling.PartialImplementation.DependencyMethodWrapperGenerator;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation.DependencyWrapperGenerators;

public abstract class BaseDependencyWrapperGenerator
{
    protected abstract IDependencyMethodClassGenerator DependencyMethodClassGenerator { get; }

    public abstract void GenerateCode(TypeBuilder builder, ITypeSymbol dependencyType);

    protected void CreateMethodWrappers(TypeBuilder builder, ITypeSymbol dependencyType, ConstructorBuilder constructorBuilder,
        string[] methodWrapperClassParameters)
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
            DependencyMethodClassGenerator.CreateMethodWrapperClass(methodWrapperClass, dependencyType.Type(), method);

            PropertyBuilder methodProperty = PropertyBuilder.Create(methodWrapperClass.Type(), name, autoGetter: true).Add(builder)
                .Public();

            constructorBuilder.AddBodyStatements(
                $"{methodProperty.Name} = new {methodWrapperClass.Name}({methodWrapperClassParameters.JoinToString(", ")});"
            );
        }
    }
}