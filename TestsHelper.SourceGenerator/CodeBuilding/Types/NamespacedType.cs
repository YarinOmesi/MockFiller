using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsHelper.SourceGenerator.CodeBuilding.Types;

[DebuggerDisplay("{Namespace}.{Name}")]
public record NamespacedType(string Namespace, string Name) : IType
{
    public virtual TypeSyntax Build() => SyntaxFactory.IdentifierName(Name);
}