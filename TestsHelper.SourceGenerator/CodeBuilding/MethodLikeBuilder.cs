using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsHelper.SourceGenerator.CodeBuilding;

public abstract class MethodLikeBuilder : MemberBuilder
{
    private List<StringWithTypes> Body { get; } = new List<StringWithTypes>();

    private readonly List<ParameterBuilder> _parameters = new();

    public void AddParameters(params ParameterBuilder[] parameterBuilders) => _parameters.AddRange(parameterBuilders);
    public void AddBodyStatement(StringWithTypesInterpolatedStringHandler stringHandler) => Body.Add(stringHandler.StringWithTypes);

    protected BlockSyntax BuildBody(BuildContext context)
    {
        return SyntaxFactory.Block(Body
            .Select(stringWithTypes => stringWithTypes.ToString(context))
            .Select(static s => SyntaxFactory.ParseStatement(s)));
    }
    protected ParameterListSyntax BuildParameters(BuildContext context)
    {
        return SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList<ParameterSyntax>(_parameters.Select(builder => builder.Build(context))));
    }
}