using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using TestsHelper.SourceGenerator.CodeBuilding;
using TestsHelper.SourceGenerator.CodeBuilding.Types;
using TestsHelper.SourceGenerator.MockFilling.PartialImplementation.DependencyMethodWrapperGenerator;
using TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Types;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation.DependencyWrapperGenerators;

public class NoWrappingDependencyWrapperGenerator : BaseDependencyWrapperGenerator
{
    protected override IDependencyMethodClassGenerator DependencyMethodClassGenerator { get; }

    public NoWrappingDependencyWrapperGenerator(IDependencyMethodClassGenerator dependencyMethodClassGenerator)
    {
        DependencyMethodClassGenerator = dependencyMethodClassGenerator;
    }

    public override void GenerateCode(TypeBuilder builder, ITypeSymbol dependencyType)
    {
        builder.Name = $"Wrapper_{dependencyType.Name}";
        builder.Public();

        //TODO: make this not coupled to moq
        PropertyBuilder mockField = PropertyBuilder.Create(Moq.Mock.Generic(dependencyType.Type()), "Mock", autoGetter: true)
            .Add(builder);
        mockField.Public();

        ConstructorBuilder.CreateAndAdd(builder)
            .InitializeFieldWithParameters((mockField, "mock"))
            .Public();
    }
}