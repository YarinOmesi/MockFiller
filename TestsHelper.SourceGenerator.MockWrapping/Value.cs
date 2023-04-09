using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DeepEqual.Syntax;
using TestsHelper.SourceGenerator.MockWrapping.Values;

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