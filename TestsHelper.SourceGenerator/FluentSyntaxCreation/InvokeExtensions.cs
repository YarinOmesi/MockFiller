using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TestsHelper.SourceGenerator.FluentSyntaxCreation;

public static partial class Extensions
{
    public static InvocationExpressionSyntax Invoke(this ExpressionSyntax instance, params ExpressionSyntax[] arguments)
    {
        return arguments switch {
            {Length: 0} => InvocationExpression(instance),
            {Length: 1} => InvocationExpression(instance, ArgumentList(SingletonSeparatedList(Argument(arguments[0])))),
            _ => InvocationExpression(instance, ArgumentList(SeparatedList(arguments.Select(Argument)))),
        };
    }
}