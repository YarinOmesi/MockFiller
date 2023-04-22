using TestsHelper.SourceGenerator.CodeBuilding.Abstractions;

namespace TestsHelper.SourceGenerator.CodeBuilding;

internal class ConstructorBuilder : MethodLikeBuilder, IConstructorBuilder
{
    private readonly ITypeBuilder _typeBuilder;

    public ConstructorBuilder(ITypeBuilder typeBuilder)
    {
        _typeBuilder = typeBuilder;
    }

    public override void Write(IIndentedStringWriter writer)
    {
        WriteModifiers(writer);
        writer.WriteSpaceSeperated(_typeBuilder.Name);
        WriteParametersAndBody(writer);
    }
}