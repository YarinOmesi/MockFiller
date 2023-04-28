using TestsHelper.SourceGenerator.CodeBuilding.Abstractions;
using TestsHelper.SourceGenerator.CodeBuilding.Types;

namespace TestsHelper.SourceGenerator.CodeBuilding;

public class FieldBuilder : MemberBuilder
{
    public string Name { get; set; } = null!;
    public IType Type { get; set; }  = null!;
    public string? Initializer { get; set; } = null;
    
    protected FieldBuilder(){}

    protected void WriteTypeAndName(IIndentedStringWriter writer)
    {
        Type.Write(writer);
        writer.Write(" ");
        writer.Write(Name);
    }

    protected void WriteInitializer(IIndentedStringWriter writer)
    {
        if (Initializer != null)
        {
            writer.Write(" ");
            writer.Write("=");
            writer.Write(Initializer);
        }
    }

    public override void Write(IIndentedStringWriter writer)
    {
        WriteModifiers(writer);
        WriteTypeAndName(writer);
        WriteInitializer(writer);
        writer.Write(";");
        writer.WriteLine();
    }

    public static FieldBuilder Create(IType type, string name, string? initializer = null) => 
        new() {Type = type, Name = name, Initializer = initializer};
}