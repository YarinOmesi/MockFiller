using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Moq;

namespace TestsHelper.SourceGenerator.MockWrapping.Values;

internal sealed record ExactValue<T>(T Value) : Value<T>
{
    private static readonly Expression<Func<T>> ItIsExpression = () => It.Is<T>(Cyber.FillValue<T>(), EqualityComparer<T>.Default);
    public T Value { get; } = Value;

    public override Expression Convert()
    {
        return ExpressionUtils.UpdateFirstArgument(ItIsExpression, Expression.Constant(Value));
    }
}