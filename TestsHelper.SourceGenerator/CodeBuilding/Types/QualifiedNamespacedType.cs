using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TestsHelper.SourceGenerator.CodeBuilding.Types;

public record QualifiedNamespacedType(string Namespace, string Name) : NamespacedType(Namespace, Name)
{
    public override TypeSyntax Build() => QualifiedName(ParseName(Namespace), IdentifierName(Name));
}