using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Moq;

namespace TestsHelper.SourceGenerator.MockWrapping;

public static class Cyber
{
    // This Method Should Not Be Ran It Used As A Filler For Types
    public static T Fill<T>() => throw new NotImplementedException();
    public static T FillValue<T>() => throw new NotImplementedException();
    public static Expression<Func<T, bool>> FillPredicate<T>() => throw new NotImplementedException();

    public static Expression<T> UpdateExpressionWithParameters<T>(Expression<T> expression, IEnumerable<Expression> arguments)
    {
        MethodCallExpression body = (MethodCallExpression) expression.Body;
        return expression.Update(body.Update(body.Object, arguments), expression.Parameters);
    }

    public static Expression CreateExpressionFor<T>(Value<T> value)
    {
        if (value is AnyValue<T>)
        {
            Expression<Func<T>> isAny = () => It.IsAny<T>();
            return isAny.Body;
        }

        if (value is ExactValue<T> exactValue)
        {
            Expression<Func<T>> itIs = () => It.Is<T>(FillValue<T>(), EqualityComparer<T>.Default);
            return UpdateFirstArgument(itIs, Expression.Constant(exactValue.Value));
        }

        if (value is PredicateValue<T> predicateValue)
        {
            Expression<Func<T>> itIs = () => It.Is<T>(FillPredicate<T>());
            return UpdateFirstArgument(itIs, predicateValue.Predicate);
        }

        throw new ArgumentOutOfRangeException(nameof(value), $"Type of value {value.GetType().Name} is unexpected");
    }

    private static MethodCallExpression UpdateFirstArgument<T>(Expression<Func<T>> original, Expression argument)
    {
        MethodCallExpression body = (MethodCallExpression) original.Body;
        List<Expression> newArguments = body.Arguments.ToList();
        newArguments[0] = argument;
        return body.Update(body.Object, newArguments);
    }
}