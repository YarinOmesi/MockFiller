using Microsoft.CodeAnalysis;
using TestsHelper.SourceGenerator.CodeBuilding.Abstractions;
using TestsHelper.SourceGenerator.CodeBuilding.Types;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation.DependencyMethodWrapperGenerator;

public interface IDependencyMethodClassGenerator
{
    public void CreateMethodWrapperClass(ITypeBuilder builder, IType dependencyTypeName, IMethodSymbol method);
}