using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsHelper.SourceGenerator.CodeBuilding.Types;

namespace TestsHelper.SourceGenerator.CodeBuilding;

public class ParameterBuilder
{
    public string Name { get; set; } = null!;
    public IType Type { get; set; } = null!;
    public string? Initializer { get; set; } = null;

    private ParameterBuilder(){}

    public ParameterSyntax Build(BuildContext context)
    {
        TypeSyntax type = Type.TryRegisterAlias(context.FileBuilder, out AliasType? aliasType) ? aliasType.Build() : Type.Build();

        return SyntaxFactory.Parameter(SyntaxFactory.Identifier(Name))
            .WithType(type)
            .WithDefault(Initializer == null ? null : SyntaxFactory.EqualsValueClause(SyntaxFactory.ParseExpression(Initializer)));
    }

    public static ParameterBuilder Create(IType type, string name, string? initializer = null) => 
        new() {Type = type, Name = name, Initializer = initializer};
}