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

    public static Expression<T> UpdateExpressionWithParameters<T>(Expression<T> expression, IEnumerable<Expression> arguments)
    {
        MethodCallExpression body = (MethodCallExpression) expression.Body;
        return expression.Update(body.Update(body.Object, arguments), expression.Parameters);
    }

    public static Expression CreateExpressionFor<T>(Value<T> value)
    {
        Expression<Func<T>> isAny = () => It.IsAny<T>();
        Expression<Func<T>> itIs = () => It.Is<T>(Fill<T>(), EqualityComparer<T>.Default);
        MethodCallExpression body = (MethodCallExpression) itIs.Body;

        if (value.IsAny)
        {
            return isAny.Body;
        }
        
        List<Expression> newArguments = body.Arguments.ToList();
        newArguments[0] = Expression.Constant(value.IsDefault ? default : value._Value);

        return body.Update(body.Object, newArguments);
    }
}
