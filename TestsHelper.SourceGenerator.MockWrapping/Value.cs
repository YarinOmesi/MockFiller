using System;
using System.Linq.Expressions;
using DeepEqual.Syntax;

namespace TestsHelper.SourceGenerator.MockWrapping;

public abstract record Value<T>
{
    public static readonly Value<T> Any = new AnyValue<T>();

    public static Value<T> Is(Expression<Func<T, bool>> predicate) => new PredicateValue<T>(predicate);
    public static Value<T> DeepEqual(T value) => new PredicateValue<T>(arg => arg.IsDeepEqual(value));

    public static implicit operator Value<T>(T value) => new ExactValue<T>(value);
}

public record AnyValue<T> : Value<T>;

public record ExactValue<T>(T Value) : Value<T>
{
    public T Value { get; } = Value;
}

public record PredicateValue<T>(Expression<Func<T, bool>> Predicate) : Value<T>
{
    public Expression<Func<T, bool>> Predicate { get; } = Predicate;
}