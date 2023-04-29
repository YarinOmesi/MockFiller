using Microsoft.CodeAnalysis;
using TestsHelper.SourceGenerator.CodeBuilding.Abstractions;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation.DependencyWrapperGenerators;

public interface IDependencyWrapperGenerator
{
    public void GenerateCode(ITypeBuilder builder, ITypeSymbol dependencyType);
}