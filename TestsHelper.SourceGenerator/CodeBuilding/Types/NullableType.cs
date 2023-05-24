using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsHelper.SourceGenerator.CodeBuilding.Types;

[DebuggerDisplay("{Type}?")]
public record NullableType(IType Type) : IType
{
    public string Namespace => Type.Namespace;

    public string Name => Type.Name;
    public TypeSyntax Build() => SyntaxFactory.NullableType(Type.Build());
}