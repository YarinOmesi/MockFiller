using System;
using System.Linq.Expressions;
using Moq;

namespace TestsHelper.SourceGenerator.MockWrapping.Values;

internal sealed record PredicateValue<T>(Expression<Func<T, bool>> Predicate) : Value<T>
{
    private static readonly Expression<Func<T>> ItIsExpression = () => It.Is<T>(Cyber.FillPredicate<T>());

    public Expression<Func<T, bool>> Predicate { get; } = Predicate;

    public override Expression Convert()
    {
        return ExpressionUtils.UpdateFirstArgument(ItIsExpression, Predicate);
    }
}