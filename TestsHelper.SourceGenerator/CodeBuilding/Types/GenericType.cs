using System.Collections.Generic;
using System.Diagnostics;

namespace TestsHelper.SourceGenerator.CodeBuilding.Types;


[DebuggerDisplay("{Namespace}.{Name}<>")]
public record GenericType(string Namespace, string Name, IReadOnlyList<IType> TypedArguments) : NamespacedType(Namespace, Name)
{
    public override void Write(IIndentedStringWriter writer)
    {
        base.Write(writer);
        writer.Write("<");
        Writer.CommaSeperated.Write(writer, TypedArguments);
        writer.Write(">");
    }
}