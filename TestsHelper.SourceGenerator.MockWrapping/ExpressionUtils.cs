using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace TestsHelper.SourceGenerator.MockWrapping;

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