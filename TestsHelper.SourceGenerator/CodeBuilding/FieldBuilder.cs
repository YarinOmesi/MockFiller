using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsHelper.SourceGenerator.CodeBuilding.Abstractions;
using TestsHelper.SourceGenerator.CodeBuilding.Types;

namespace TestsHelper.SourceGenerator.CodeBuilding;

public class FieldBuilder : MemberBuilder
{
    public string Name { get; set; } = null!;
    public IType Type { get; set; } = null!;
    public string? Initializer { get; set; } = null;

    protected FieldBuilder()
    {
    }

    protected EqualsValueClauseSyntax? BuildInitializer()
    {
        return Initializer == null ? null : SyntaxFactory.EqualsValueClause(SyntaxFactory.ParseExpression(Initializer));
    }

    public override MemberDeclarationSyntax Build()
    {
        return SyntaxFactory.FieldDeclaration(
            SyntaxFactory.VariableDeclaration(Type.Build())
                .AddVariables(
                    SyntaxFactory.VariableDeclarator(Name).WithInitializer(BuildInitializer())
                )
        ).WithModifiers(BuildModifiers());
        //.WithSemicolonToken();
    }

    public static FieldBuilder Create(IType type, string name, string? initializer = null) =>
        new() {Type = type, Name = name, Initializer = initializer};
}