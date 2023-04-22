using System.Diagnostics;

namespace TestsHelper.SourceGenerator.CodeBuilding.Types;

[DebuggerDisplay("{Namespace}.{Name}")]
public record NamespacedType(string Namespace, string Name) : IType
{
    public virtual void Write(IIndentedStringWriter writer) => writer.Write(Name);
}