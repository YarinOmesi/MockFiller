using System.Diagnostics;

namespace TestsHelper.SourceGenerator.CodeBuilding.Types;

[DebuggerDisplay("void")]
public sealed class VoidType : IType
{
    public static readonly VoidType Instance = new();
    private VoidType()
    {
        
    }
    public string Namespace => string.Empty;

    public string Name => "void";

    public void Write(IIndentedStringWriter writer)
    {
        writer.Write(Name);
    }
}