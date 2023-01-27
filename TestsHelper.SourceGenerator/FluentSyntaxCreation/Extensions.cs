using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;


namespace TestsHelper.SourceGenerator.FluentSyntaxCreation;

public static class Extensions
{
    private static readonly SyntaxToken SemicolonToken = Token(SyntaxKind.SemicolonToken);

    public static GenericNameSyntax Generic(this SyntaxToken type, params TypeSyntax[] arguments) =>
        GenericName(type, TypeArgumentList(SeparatedList(arguments)));

    public static GenericNameSyntax Generic(this SyntaxToken type, IEnumerable<TypeSyntax> arguments) =>
        GenericName(type, TypeArgumentList(SeparatedList(arguments)));

    public static GenericNameSyntax Generic(this string type, IEnumerable<TypeSyntax> arguments) => Identifier(type).Generic(arguments);

    public static GenericNameSyntax Generic(this string type, params string[] arguments) =>
        type.Generic(arguments.Select(IdentifierName));

    public static GenericNameSyntax Generic(this string type, TypeSyntax argument) =>
        GenericName(Identifier(type), TypeArgumentList(SingletonSeparatedList(argument)));

    public static GenericNameSyntax Generic(this string type, string typedName) =>
        GenericName(Identifier(type), TypeArgumentList(SingletonSeparatedList<TypeSyntax>(IdentifierName(typedName))));

    public static MemberAccessExpressionSyntax AccessMember(this string instance, string member) =>
        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName(instance), IdentifierName(member));

    public static MemberAccessExpressionSyntax AccessMember(this string instance, SimpleNameSyntax member) =>
        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName(instance), member);

    public static MemberAccessExpressionSyntax AccessMember(this ExpressionSyntax instance, string member) =>
        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, instance, IdentifierName(member));

    public static InvocationExpressionSyntax Invoke(this ExpressionSyntax instance) => InvocationExpression(instance);

    public static InvocationExpressionSyntax Invoke(this ExpressionSyntax instance, IEnumerable<ExpressionSyntax> arguments)
    {
        return instance.Invoke(arguments.ToArray());
    }

    public static InvocationExpressionSyntax Invoke(this ExpressionSyntax instance, params ExpressionSyntax[] arguments)
    {
        return InvocationExpression(instance, ArgumentList(SeparatedList(arguments.Select(Argument))));
    }

    public static InvocationExpressionSyntax Invoke(this ExpressionSyntax instance, ExpressionSyntax argument)
    {
        return InvocationExpression(instance, ArgumentList(SingletonSeparatedList(Argument(argument))));
    }

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

    public static ImplicitArrayCreationExpressionSyntax ImplicitCreation(this InitializerExpressionSyntax initializerExpressionSyntax)
    {
        return ImplicitArrayCreationExpression(initializerExpressionSyntax);
    }

    public static VariableDeclarationSyntax DeclareVariable(this TypeSyntax type, VariableDeclaratorSyntax variable)
    {
        return VariableDeclaration(type, SingletonSeparatedList(variable));
    }

    public static VariableDeclarationSyntax DeclareVariable(this TypeSyntax type, string name, ExpressionSyntax? initializer = null)
    {
        if (initializer == null)
            return type.DeclareVariable(VariableDeclarator(name));

        return type.DeclareVariable(VariableDeclarator(name).WithInitializer(EqualsValueClause(initializer)));
    }

    public static AssignmentExpressionSyntax Assign(this ExpressionSyntax container, ExpressionSyntax value)
    {
        return AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, container, value);
    }

    public static AssignmentExpressionSyntax Assign(this string container, string value) => IdentifierName(container).Assign(IdentifierName(value));

    public static ExpressionStatementSyntax ToStatement(this ExpressionSyntax expressionSyntax) => ExpressionStatement(expressionSyntax);

    public static FieldDeclarationSyntax AddModifier(this FieldDeclarationSyntax field, SyntaxKind kind) => field.AddModifiers(Token(kind));

    public static FieldDeclarationSyntax AddModifiers(this FieldDeclarationSyntax field, params SyntaxKind[] kinds)
    {
        return kinds.Aggregate(field, (current, syntaxKind) => current.AddModifier(syntaxKind));
    }

    public static ObjectCreationExpressionSyntax New(this TypeSyntax typeSyntax, params ExpressionSyntax[] arguments)
    {
        SeparatedSyntaxList<ArgumentSyntax> argumentsList = arguments.Length == 1
            ? SingletonSeparatedList(Argument(arguments[0]))
            : SeparatedList(arguments.Select(Argument));

        return ObjectCreationExpression(typeSyntax)
            .WithArgumentList(ArgumentList(argumentsList));
    }

    public static ParameterSyntax Parameter(this string name, TypeSyntax type) => SyntaxFactory.Parameter(Identifier(name)).WithType(type);

    public static FieldDeclarationSyntax DeclareField(this string type, string name, ExpressionSyntax? initializer = null) =>
        IdentifierName(type).DeclareField(name, initializer);

    public static FieldDeclarationSyntax DeclareField(this TypeSyntax type, string name, ExpressionSyntax? initializer = null) =>
        FieldDeclaration(type.DeclareVariable(name, initializer));

    public static ReturnStatementSyntax Return(this ExpressionSyntax expression) => ReturnStatement(expression).WithSemicolonToken(SemicolonToken);
}