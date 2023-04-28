using System.Diagnostics.Contracts;
using TestsHelper.SourceGenerator.CodeBuilding.Abstractions;
using TestsHelper.SourceGenerator.CodeBuilding.Types;

namespace TestsHelper.SourceGenerator.CodeBuilding;

internal class MethodBuilder : MethodLikeBuilder
{
    public string Name { get; set; } = null!;
    public IType ReturnType { get; set; } = null!;

    private MethodBuilder(){}
 
    public override void Write(IIndentedStringWriter writer)
    {
        WriteModifiers(writer);
        ReturnType.Write(writer);
        writer.Write(" ");
        writer.Write(Name);
        WriteParametersAndBody(writer);
    }

    [Pure]
    public static MethodBuilder Create(IType returnType, string name, params IParameterBuilder[] parameters)
    {
        var methodBuilder = new MethodBuilder {Name = name, ReturnType = returnType};
        methodBuilder.AddParameters(parameters);
        return methodBuilder;
    }
}