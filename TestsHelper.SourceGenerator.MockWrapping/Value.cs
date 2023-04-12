using System;
using System.Linq.Expressions;
using DeepEqual.Syntax;
using TestsHelper.SourceGenerator.MockWrapping.Values;

namespace TestsHelper.SourceGenerator.MockWrapping;

public abstract record Value<T>
{
    public static readonly Value<T> Any = new AnyValue<T>();

    public static Value<T> Is(Expression<Func<T, bool>> predicate) => new PredicateValue<T>(predicate);
    public static Value<T> Is(T value) => value;
    public static Value<T> DeepEqual(T value) => new PredicateValue<T>(arg => arg.IsDeepEqual(value));

    public static implicit operator Value<T>(T value) => new ExactValue<T>(value);
}