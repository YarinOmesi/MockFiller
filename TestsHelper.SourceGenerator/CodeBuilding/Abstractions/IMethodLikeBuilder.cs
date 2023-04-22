using System.Collections.Generic;

namespace TestsHelper.SourceGenerator.CodeBuilding.Abstractions;

public interface IMethodLikeBuilder : IMemberBuilder
{
    public IReadOnlyList<IParameterBuilder> Parameters { get; }
    public void AddParameters(params IParameterBuilder[] parameterBuilders);
    public void AddBodyStatements(params string[] statements);
}