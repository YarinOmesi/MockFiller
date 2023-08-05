using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsHelper.SourceGenerator.CodeBuilding.Types;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TestsHelper.SourceGenerator.CodeBuilding;

public class ParameterBuilder
{
    public string Name { get; set; } = null!;
    public IType Type { get; set; } = null!;
    public StringWithTypes Initializer { get; set; } = StringWithTypes.Empty;

    private ParameterBuilder(){}

    public ParameterSyntax Build(BuildContext context)
    {
        TypeSyntax type = Type.TryRegisterAlias(context.FileBuilder).Build();

        return Parameter(Identifier(Name))
            .WithType(type)
            .WithDefault(Initializer.IsEmpty ? null : EqualsValueClause(ParseExpression(Initializer.ToString(context.FileBuilder))));
    }

    public static ParameterBuilder Create(IType type, string name, StringWithTypes? initializer = null) => 
        new() {Type = type, Name = name, Initializer = initializer ?? StringWithTypes.Empty};
}