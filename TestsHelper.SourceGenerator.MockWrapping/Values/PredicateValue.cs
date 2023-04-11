using System;
using System.Linq.Expressions;

namespace TestsHelper.SourceGenerator.MockWrapping.Values;

internal sealed record PredicateValue<T>(Expression<Func<T, bool>> Predicate) : Value<T>
{
    public Expression<Func<T, bool>> Predicate { get; } = Predicate;
}