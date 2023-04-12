using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OneOf;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TestsHelper.SourceGenerator.FluentSyntaxCreation;

public static partial class Extensions
{
    public static MemberAccessExpressionSyntax AccessMember(this string instance, OneOf<string, SimpleNameSyntax> member) =>
        IdentifierName(instance).AccessMember(member.ToSyntax());

    public static MemberAccessExpressionSyntax AccessMember(this ExpressionSyntax instance, OneOf<string, SimpleNameSyntax> member) =>
        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, instance, member.ToSyntax());

    private static SimpleNameSyntax ToSyntax(this OneOf<string, SimpleNameSyntax> member) => member.Match(IdentifierName, ReturnInput);
}