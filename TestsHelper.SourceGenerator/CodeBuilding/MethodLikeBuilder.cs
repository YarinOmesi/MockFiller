using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsHelper.SourceGenerator.CodeBuilding.Abstractions;

namespace TestsHelper.SourceGenerator.CodeBuilding;

public abstract class MethodLikeBuilder : MemberBuilder, IMethodLikeBuilder
{
    public IReadOnlyList<IParameterBuilder> Parameters => _parameters;
    private List<string> Body { get; } = new List<string>();

    private readonly List<IParameterBuilder> _parameters = new();

    public void AddParameters(params IParameterBuilder[] parameterBuilders) => _parameters.AddRange(parameterBuilders);
    public void AddBodyStatements(params string[] statements) => Body.AddRange(statements);

    protected BlockSyntax BuildBody() => SyntaxFactory.Block(Body.Select(s=> SyntaxFactory.ParseStatement(s)));

    protected ParameterListSyntax BuildParameters()
    {
        return SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList<ParameterSyntax>(Parameters.Select(builder => builder.Build())));
    }
}