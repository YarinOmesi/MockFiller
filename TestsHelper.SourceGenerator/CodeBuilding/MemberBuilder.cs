using System.Collections.Generic;
using TestsHelper.SourceGenerator.CodeBuilding.Abstractions;

namespace TestsHelper.SourceGenerator.CodeBuilding;

public abstract class MemberBuilder : IMemberBuilder
{
    public List<string> Modifiers { get; } = new List<string>();

    public void AddModifiers(params string[] modifiers) => Modifiers.AddRange(modifiers);

    protected void WriteModifiers(IIndentedStringWriter writer)
    {
        if (Modifiers.Count != 0)
        {
            writer.WriteSpaceSeperated(Modifiers.ToArray());
            writer.Write(" ");
        }
    }

    public abstract void Write(IIndentedStringWriter writer);
}