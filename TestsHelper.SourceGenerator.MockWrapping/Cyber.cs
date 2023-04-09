using System;
using System.Collections.Generic;
using System.Linq.Expressions;

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
}