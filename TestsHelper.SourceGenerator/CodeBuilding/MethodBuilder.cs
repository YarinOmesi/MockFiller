using System.Diagnostics.Contracts;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsHelper.SourceGenerator.CodeBuilding.Abstractions;
using TestsHelper.SourceGenerator.CodeBuilding.Types;

namespace TestsHelper.SourceGenerator.CodeBuilding;

internal class MethodBuilder : MethodLikeBuilder
{
    public string Name { get; set; } = null!;
    public IType ReturnType { get; set; } = null!;

    private MethodBuilder()
    {
    }
    
    public override MemberDeclarationSyntax Build()
    {
        return SyntaxFactory.MethodDeclaration(returnType: ReturnType.Build(), identifier: SyntaxFactory.Identifier(Name))
            .WithModifiers(BuildModifiers())
            .WithBody(BuildBody())
            .WithParameterList(BuildParameters());
    }

    [Pure]
    public static MethodBuilder Create(IType returnType, string name, params IParameterBuilder[] parameters)
    {
        var methodBuilder = new MethodBuilder {Name = name, ReturnType = returnType};
        methodBuilder.AddParameters(parameters);
        return methodBuilder;
    }
}