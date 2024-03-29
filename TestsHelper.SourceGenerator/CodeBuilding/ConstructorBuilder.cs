using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsHelper.SourceGenerator.CodeBuilding;

public class ConstructorBuilder : MethodLikeBuilder
{
    private readonly TypeBuilder _typeBuilder;

    private ConstructorBuilder(TypeBuilder typeBuilder)
    {
        _typeBuilder = typeBuilder;
    }

    public override MemberDeclarationSyntax Build(BuildContext context)
    {
        return SyntaxFactory.ConstructorDeclaration(SyntaxFactory.Identifier(_typeBuilder.Name))
            .WithModifiers(BuildModifiers())
            .WithParameterList(BuildParameters(context))
            .WithBody(BuildBody(context));
    }

    public static ConstructorBuilder CreateAndAdd(TypeBuilder type, params ParameterBuilder[] parameters)
    {
        var constructorBuilder = new ConstructorBuilder(type);
        constructorBuilder.AddParameters(parameters);
        return constructorBuilder.Add(type);
    }
}