using TestsHelper.SourceGenerator.CodeBuilding.Abstractions;
using TestsHelper.SourceGenerator.CodeBuilding.Types;

namespace TestsHelper.SourceGenerator.CodeBuilding;

public class ParameterBuilder : IParameterBuilder
{
    public string Name { get; set; } = null!;
    public IType Type { get; set; } = null!;
    public string? Initializer { get; set; } = null;

    public void Write(IIndentedStringWriter writer)
    {
        Type.Write(writer);
        writer.Write(" ");
        writer.Write(Name);

        if (Initializer != null)
        {
            writer.Write(" ");
            writer.Write("=");
            writer.Write(Initializer);
        }
    }
}