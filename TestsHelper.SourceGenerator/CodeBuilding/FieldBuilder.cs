using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsHelper.SourceGenerator.CodeBuilding.Types;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TestsHelper.SourceGenerator.CodeBuilding;

public class FieldBuilder : MemberBuilder
{
    public string Name { get; set; } = null!;
    public IType Type { get; set; } = null!;
    public StringWithTypes Initializer { get; set; } = StringWithTypes.Empty;

    protected FieldBuilder() { }

    protected EqualsValueClauseSyntax? BuildInitializer(BuildContext context)
    {
        return Initializer.IsEmpty ? null : EqualsValueClause(ParseExpression(Initializer.ToString(context.FileBuilder)));
    }

    public override MemberDeclarationSyntax Build(BuildContext context)
    {
        TypeSyntax type = Type.TryRegisterAlias(context.FileBuilder).Build();

        return FieldDeclaration(VariableDeclaration(type)
            .AddVariables(VariableDeclarator(Name).WithInitializer(BuildInitializer(context)))).WithModifiers(BuildModifiers());
    }

    public static FieldBuilder Create(IType type, string name, StringWithTypes? initializer = null) =>
        new() {Type = type, Name = name, Initializer = initializer ?? StringWithTypes.Empty};
}