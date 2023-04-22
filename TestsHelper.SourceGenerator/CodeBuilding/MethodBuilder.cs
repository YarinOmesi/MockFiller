using TestsHelper.SourceGenerator.CodeBuilding.Abstractions;
using TestsHelper.SourceGenerator.CodeBuilding.Types;

namespace TestsHelper.SourceGenerator.CodeBuilding;

internal class MethodBuilder : MethodLikeBuilder, IMethodBuilder
{
    public string Name { get; set; } = null!;
    public IType ReturnType { get; set; } = null!;
 
    public override void Write(IIndentedStringWriter writer)
    {
        WriteModifiers(writer);
        ReturnType.Write(writer);
        writer.Write(" ");
        writer.Write(Name);
        WriteParametersAndBody(writer);
    }
}