using Microsoft.CodeAnalysis;
using TestsHelper.SourceGenerator.CodeBuilding;
using TestsHelper.SourceGenerator.CodeBuilding.Types;
using TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Types;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation.DependencyMethodWrapperGenerator;

public class NoWrappingDependencyMethodGenerator : IDependencyMethodClassGenerator
{
    public void CreateMethodWrapperClass(TypeBuilder builder, IType dependencyTypeName, IMethodSymbol method)
    {
        builder.Name = $"Method_{method.Name}";
        builder.Public();

        FieldBuilder mockField = FieldBuilder.Create(Moq.Mock.Generic(dependencyTypeName), "_mock").Add(builder);
        mockField.Private().Readonly();

        ConstructorBuilder.CreateAndAdd(builder)
            .InitializeFieldWithParameters((mockField, "mock"))
            .Public();
    }
}