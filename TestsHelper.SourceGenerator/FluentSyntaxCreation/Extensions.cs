using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OneOf;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;


namespace TestsHelper.SourceGenerator.FluentSyntaxCreation;

public static partial class Extensions
{
    private static readonly SyntaxToken SemicolonToken = Token(SyntaxKind.SemicolonToken);
    private static readonly SyntaxToken VarIdentifier = Identifier(TriviaList(), SyntaxKind.VarKeyword, "var", "var", TriviaList());

    private static T ReturnInput<T>(T arg) => arg;

    public static BinaryExpressionSyntax Coalesce(this ExpressionSyntax left, ExpressionSyntax right)
    {
        // left ?? right
        return BinaryExpression(SyntaxKind.CoalesceExpression, left, right);
    }

    public static IdentifierNameSyntax Name(this SyntaxToken node) => IdentifierName(node);

    public static InitializerExpressionSyntax ArrayInitializer(this IEnumerable<ExpressionSyntax> values)
    {
        return InitializerExpression(SyntaxKind.ArrayInitializerExpression, SeparatedList(values));
    }

    public static ImplicitArrayCreationExpressionSyntax ImplicitCreation(this InitializerExpressionSyntax initializerExpressionSyntax) => 
        ImplicitArrayCreationExpression(initializerExpressionSyntax);

    public static AssignmentExpressionSyntax Assign(this ExpressionSyntax container, ExpressionSyntax value) => 
        AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, container, value);

    public static AssignmentExpressionSyntax Assign(this string container, OneOf<string, ExpressionSyntax> value) => 
        IdentifierName(container).Assign(value.Match(IdentifierName, ReturnInput));

    public static ExpressionStatementSyntax ToStatement(this ExpressionSyntax expressionSyntax) => ExpressionStatement(expressionSyntax);

    public static FieldDeclarationSyntax AddModifiers(this FieldDeclarationSyntax field, params SyntaxKind[] kinds) => field.AddModifiers(kinds.Select(Token).ToArray());
    public static ClassDeclarationSyntax AddModifiers(this ClassDeclarationSyntax field, params SyntaxKind[] kinds) => field.AddModifiers(kinds.Select(Token).ToArray());
    public static MethodDeclarationSyntax AddModifiers(this MethodDeclarationSyntax field, params SyntaxKind[] kinds) => field.AddModifiers(kinds.Select(Token).ToArray());
    public static PropertyDeclarationSyntax AddModifiers(this PropertyDeclarationSyntax field, params SyntaxKind[] kinds) => field.AddModifiers(kinds.Select(Token).ToArray());
    public static ConstructorDeclarationSyntax AddModifiers(this ConstructorDeclarationSyntax field, params SyntaxKind[] kinds) => field.AddModifiers(kinds.Select(Token).ToArray());

    public static ObjectCreationExpressionSyntax New(this TypeSyntax typeSyntax, params ExpressionSyntax[] arguments)
    {
        SeparatedSyntaxList<ArgumentSyntax> argumentsList = arguments.Length == 1
            ? SingletonSeparatedList(Argument(arguments[0]))
            : SeparatedList(arguments.Select(Argument));

        return ObjectCreationExpression(typeSyntax)
            .WithArgumentList(ArgumentList(argumentsList));
    }

    public static ObjectCreationExpressionSyntax New(this string type, params ExpressionSyntax[] arguments) => IdentifierName(type).New(arguments);

    public static ParameterSyntax Parameter(this string name, TypeSyntax type, ExpressionSyntax? defaultValue = null)
    {
        ParameterSyntax parameterSyntax = SyntaxFactory.Parameter(Identifier(name)).WithType(type);
        
        return defaultValue != null ? parameterSyntax.WithDefault(EqualsValueClause(defaultValue)) : parameterSyntax;
    }

    public static MethodDeclarationSyntax AddParameters(this MethodDeclarationSyntax method, params ParameterSyntax[] parameters) => 
        method.AddParameterListParameters(parameters);

    public static FieldDeclarationSyntax DeclareField(this string type, string name, ExpressionSyntax? initializer = null) =>
        IdentifierName(type).DeclareField(name, initializer);

    public static FieldDeclarationSyntax DeclareField(this TypeSyntax type, string name, ExpressionSyntax? initializer = null) =>
        FieldDeclaration(type.DeclareVariable(name, initializer));

    public static ReturnStatementSyntax Return(this ExpressionSyntax expression) =>
        ReturnStatement(expression).WithSemicolonToken(SemicolonToken);
}