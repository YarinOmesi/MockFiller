using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsHelper.SourceGenerator.CodeBuilding.Types;

[DebuggerDisplay("void")]
public sealed class VoidType : IType
{
    public static readonly VoidType Instance = new();
    private VoidType() { }

    public string Namespace => string.Empty;

    public string Name => "void";
    
    public TypeSyntax Build() => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword));
}