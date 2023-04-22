using TestsHelper.SourceGenerator.CodeBuilding.Types;

namespace TestsHelper.SourceGenerator.CodeBuilding.Abstractions;

public interface IMethodBuilder : IMethodLikeBuilder
{
    public string Name { get; set; }

    public IType ReturnType { get; set; }
}