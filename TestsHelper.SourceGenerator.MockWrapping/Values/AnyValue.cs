using System;
using System.Linq.Expressions;
using Moq;

namespace TestsHelper.SourceGenerator.MockWrapping.Values;

public record AnyValue<T> : Value<T>
{
    private static readonly Expression<Func<T>> IsAnyExpression = () => It.IsAny<T>();

    public override Expression Convert() => IsAnyExpression.Body;
}