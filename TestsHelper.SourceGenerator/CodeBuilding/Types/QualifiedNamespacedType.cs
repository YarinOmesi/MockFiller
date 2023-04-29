using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsHelper.SourceGenerator.CodeBuilding.Types;

public record QualifiedNamespacedType(string Namespace, string Name) : NamespacedType(Namespace, Name)
{
    public override TypeSyntax Build()
    {
        return SyntaxFactory.QualifiedName(SyntaxFactory.ParseName(Namespace), SyntaxFactory.IdentifierName(Name));
    }
}