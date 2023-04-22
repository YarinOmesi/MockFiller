using System.Collections.Generic;
using TestsHelper.SourceGenerator.CodeBuilding.Abstractions;
using TestsHelper.SourceGenerator.CodeBuilding.Types;

namespace TestsHelper.SourceGenerator.CodeBuilding;

internal class PropertyBuilder : IPropertyBuilder
{
    public List<string> Modifiers { get; } = new List<string>();

    public string Name { get; set; } = null!;

    public IType Type { get; set; } = null!;

    public string? Initializer { get; set; } = null;

    public bool AutoGetter { get; set; } = true;

    public bool AutoSetter { get; set; } = true;

    public void AddModifiers(params string[] modifiers) => Modifiers.AddRange(modifiers);

    public void Write(IIndentedStringWriter writer)
    {
        if (Modifiers.Count > 0)
        {
            writer.WriteSpaceSeperated(Modifiers.ToArray());
            writer.Write(" ");
        }
        Type.Write(writer);
        writer.Write(" ");
        writer.Write(Name);

        if (Initializer != null)
        {
            writer.Write(" ");
            writer.Write("=");
            writer.Write(Initializer);
        }
        
        writer.Write(" { ");
        if(AutoGetter) writer.Write("get;");
        if(AutoSetter) writer.Write("set;");
        writer.Write(" }");
        writer.WriteLine();
    }
}