using Microsoft.CodeAnalysis;
using TestsHelper.SourceGenerator.CodeBuilding;
using TestsHelper.SourceGenerator.CodeBuilding.Types;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation.DependencyMethodWrapperGenerator;

public interface IDependencyMethodWrapperClassGenerator
{
    public void Generate(TypeBuilder builder, IType dependencyTypeName, IMethodSymbol method);
}