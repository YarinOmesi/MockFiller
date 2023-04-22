using TestsHelper.SourceGenerator.CodeBuilding.Abstractions;

namespace TestsHelper.SourceGenerator.CodeBuilding.Types;

public interface IType : IWritable
{
    public string Namespace { get; }
    public string Name { get; }
}