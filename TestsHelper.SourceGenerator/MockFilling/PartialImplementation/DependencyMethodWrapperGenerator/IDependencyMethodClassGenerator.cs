using Microsoft.CodeAnalysis;
using TestsHelper.SourceGenerator.CodeBuilding;
using TestsHelper.SourceGenerator.CodeBuilding.Types;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation.DependencyMethodWrapperGenerator;

public interface IDependencyMethodClassGenerator
{
    public void CreateMethodWrapperClass(TypeBuilder builder, IType dependencyTypeName, IMethodSymbol method);
}