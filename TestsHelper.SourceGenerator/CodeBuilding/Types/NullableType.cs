using System.Diagnostics;

namespace TestsHelper.SourceGenerator.CodeBuilding.Types;

[DebuggerDisplay("{Type}?")]
public record NullableType(IType Type):IType
{
    public string Namespace => Type.Namespace;

    public string Name => Type.Name;

    public void Write(IIndentedStringWriter writer)
    {
        Type.Write(writer);
        writer.Write("?");
    }
}