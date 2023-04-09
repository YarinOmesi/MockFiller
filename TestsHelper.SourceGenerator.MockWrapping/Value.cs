using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DeepEqual.Syntax;
using Moq;

namespace TestsHelper.SourceGenerator.MockWrapping;

public abstract record Value<T>
{
    public static readonly Value<T> Any = new AnyValue<T>();

    public abstract Expression Convert();

    public static Value<T> Is(Expression<Func<T, bool>> predicate) => new PredicateValue<T>(predicate);
    public static Value<T> Is(T value) => value;
    public static Value<T> DeepEqual(T value) => new PredicateValue<T>(arg => arg.IsDeepEqual(value));
    
    public static Expression ConvertValueOrAny(Value<T> value) => (value ?? Any).Convert();

    public static implicit operator Value<T>(T value) => new ExactValue<T>(value);
}

public record AnyValue<T> : Value<T>
{
    private static readonly Expression<Func<T>> IsAnyExpression = () => It.IsAny<T>();

    public override Expression Convert() => IsAnyExpression.Body;
}

public record ExactValue<T>(T Value) : Value<T>
{
    private static readonly Expression<Func<T>> ItIsExpression = () => It.Is<T>(Cyber.FillValue<T>(), EqualityComparer<T>.Default);
    public T Value { get; } = Value;

    public override Expression Convert()
    {
        return ExpressionUtils.UpdateFirstArgument(ItIsExpression, Expression.Constant(Value));
    }
}

public record PredicateValue<T>(Expression<Func<T, bool>> Predicate) : Value<T>
{
    private static readonly Expression<Func<T>> ItIsExpression = () => It.Is<T>(Cyber.FillPredicate<T>());

    public Expression<Func<T, bool>> Predicate { get; } = Predicate;

    public override Expression Convert()
    {
        return ExpressionUtils.UpdateFirstArgument(ItIsExpression, Predicate);
    }
}

internal static class ExpressionUtils
{
    public static MethodCallExpression UpdateFirstArgument<T>(Expression<Func<T>> original, Expression argument)
    {
        MethodCallExpression body = (MethodCallExpression) original.Body;
        List<Expression> newArguments = body.Arguments.ToList();
        newArguments[0] = argument;
        return body.Update(body.Object, newArguments);
    }
}