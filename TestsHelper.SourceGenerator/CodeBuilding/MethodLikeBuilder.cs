using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsHelper.SourceGenerator.CodeBuilding;

public abstract class MethodLikeBuilder : MemberBuilder
{
    public IReadOnlyList<ParameterBuilder> Parameters => _parameters;
    private List<string> Body { get; } = new List<string>();

    private readonly List<ParameterBuilder> _parameters = new();

    public void AddParameters(params ParameterBuilder[] parameterBuilders) => _parameters.AddRange(parameterBuilders);
    public void AddBodyStatements(params string[] statements) => Body.AddRange(statements);

    protected BlockSyntax BuildBody() => SyntaxFactory.Block(Body.Select(s=> SyntaxFactory.ParseStatement(s)));

    protected ParameterListSyntax BuildParameters(BuildContext context)
    {
        return SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList<ParameterSyntax>(Parameters.Select(builder => builder.Build(context))));
    }
}