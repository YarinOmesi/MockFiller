using System.Diagnostics.Contracts;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsHelper.SourceGenerator.CodeBuilding.Abstractions;

namespace TestsHelper.SourceGenerator.CodeBuilding;

public class ConstructorBuilder : MethodLikeBuilder
{
    private readonly ITypeBuilder _typeBuilder;

    private ConstructorBuilder(ITypeBuilder typeBuilder)
    {
        _typeBuilder = typeBuilder;
    }

    public override MemberDeclarationSyntax Build()
    {
        return SyntaxFactory.ConstructorDeclaration(SyntaxFactory.Identifier(_typeBuilder.Name))
            .WithModifiers(BuildModifiers())
            .WithParameterList(BuildParameters())
            .WithBody(BuildBody());
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