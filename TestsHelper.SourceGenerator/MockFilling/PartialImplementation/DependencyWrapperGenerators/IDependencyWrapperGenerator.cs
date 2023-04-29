using Microsoft.CodeAnalysis;
using TestsHelper.SourceGenerator.CodeBuilding;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation.DependencyWrapperGenerators;

public interface IDependencyWrapperGenerator
{
    public void GenerateCode(TypeBuilder builder, ITypeSymbol dependencyType);
}