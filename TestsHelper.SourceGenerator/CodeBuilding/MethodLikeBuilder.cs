using System.Collections.Generic;
using TestsHelper.SourceGenerator.CodeBuilding.Abstractions;

namespace TestsHelper.SourceGenerator.CodeBuilding;

public abstract class MethodLikeBuilder : MemberBuilder, IMethodLikeBuilder
{
    public IReadOnlyList<IParameterBuilder> Parameters => _parameters;
    protected List<string> Body { get; } = new List<string>();


    protected readonly List<IParameterBuilder> _parameters = new();

    public void AddParameters(params IParameterBuilder[] parameterBuilders) => _parameters.AddRange(parameterBuilders);
    public void AddBodyStatements(params string[] statements) => Body.AddRange(statements);

    protected void WriteParametersAndBody(IIndentedStringWriter writer)
    {
        writer.Write("(");
        Writer.CommaSeperated.Write(writer, Parameters);
        writer.Write(")");
        writer.WriteLine();
        Writer.Block.Write(writer, Body);
    }
}