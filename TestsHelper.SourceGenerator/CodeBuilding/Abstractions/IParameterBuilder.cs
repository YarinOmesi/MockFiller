using TestsHelper.SourceGenerator.CodeBuilding.Types;

namespace TestsHelper.SourceGenerator.CodeBuilding.Abstractions;

public interface IParameterBuilder : IWritable
{
    public string Name { get; set; }
    public IType Type { get; set; }
    public string? Initializer { get; set; }
}