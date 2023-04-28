using System.Diagnostics.Contracts;
using TestsHelper.SourceGenerator.CodeBuilding.Abstractions;

namespace TestsHelper.SourceGenerator.CodeBuilding;

public class ConstructorBuilder : MethodLikeBuilder
{
    private readonly ITypeBuilder _typeBuilder;

    private ConstructorBuilder(ITypeBuilder typeBuilder)
    {
        _typeBuilder = typeBuilder;
    }

    public override void Write(IIndentedStringWriter writer)
    {
        WriteModifiers(writer);
        writer.WriteSpaceSeperated(_typeBuilder.Name);
        WriteParametersAndBody(writer);
    }

    public static ConstructorBuilder CreateAndAdd(ITypeBuilder type, params IParameterBuilder[] parameters) =>
        Create(type, parameters).Add(type);

    [Pure]
    public static ConstructorBuilder Create(ITypeBuilder type, params IParameterBuilder[] parameters)
    {
        var constructorBuilder = new ConstructorBuilder(type);
        constructorBuilder.AddParameters(parameters);
        return constructorBuilder;
    }
}