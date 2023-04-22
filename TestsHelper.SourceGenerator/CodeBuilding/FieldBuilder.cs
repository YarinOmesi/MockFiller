using TestsHelper.SourceGenerator.CodeBuilding.Abstractions;
using TestsHelper.SourceGenerator.CodeBuilding.Types;

namespace TestsHelper.SourceGenerator.CodeBuilding;

internal class FieldBuilder : MemberBuilder, IFieldBuilder
{
    public string Name { get; set; } = null!;
    public IType Type { get; set; }  = null!;
    public string? Initializer { get; set; } = null;

    public override void Write(IIndentedStringWriter writer)
    {
        WriteModifiers(writer);

        Type.Write(writer);
        writer.Write(" ");
        writer.Write(Name);

        if (Initializer != null)
        {
            writer.Write(" ");
            writer.Write("=");
            writer.Write(Initializer);
        }

        writer.Write(";");
        writer.WriteLine();
    }
}