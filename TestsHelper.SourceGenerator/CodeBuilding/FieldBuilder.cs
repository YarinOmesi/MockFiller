using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsHelper.SourceGenerator.CodeBuilding.Types;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TestsHelper.SourceGenerator.CodeBuilding;

public class FieldBuilder : MemberBuilder
{
    public string Name { get; set; } = null!;
    public IType Type { get; set; } = null!;
    public string? Initializer { get; set; } = null;

    protected FieldBuilder() { }

    protected EqualsValueClauseSyntax? BuildInitializer() => Initializer == null ? null : EqualsValueClause(ParseExpression(Initializer));

    public override MemberDeclarationSyntax Build(BuildContext context)
    {
        TypeSyntax type = Type.TryRegisterAlias(context.FileBuilder).Build();

        return FieldDeclaration(VariableDeclaration(type)
            .AddVariables(VariableDeclarator(Name).WithInitializer(BuildInitializer()))).WithModifiers(BuildModifiers());
    }

    public static FieldBuilder Create(IType type, string name, string? initializer = null) =>
        new() {Type = type, Name = name, Initializer = initializer};
}