using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TestsHelper.SourceGenerator.FluentSyntaxCreation;

public static partial class Extensions
{
    public static PropertyDeclarationSyntax AddAccessors(this PropertyDeclarationSyntax property, params SyntaxKind[] kinds)
    {
        return property.WithAccessorList(AccessorList(List(
            kinds.Select(kind => AccessorDeclaration(kind).WithSemicolonToken(SemicolonToken))
        )));
    }
}