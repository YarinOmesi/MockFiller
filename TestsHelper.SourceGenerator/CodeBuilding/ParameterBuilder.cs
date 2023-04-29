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

    public ParameterSyntax Build()
    {
        return SyntaxFactory.Parameter(SyntaxFactory.Identifier(Name))
            .WithType(Type.Build())
            .WithDefault(Initializer == null ? null : SyntaxFactory.EqualsValueClause(SyntaxFactory.ParseExpression(Initializer)));
    }

    public static ParameterBuilder Create(IType type, string name, string? initializer = null) => 
        new() {Type = type, Name = name, Initializer = initializer};
}