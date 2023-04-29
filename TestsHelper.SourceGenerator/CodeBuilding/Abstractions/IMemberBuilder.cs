using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsHelper.SourceGenerator.CodeBuilding.Abstractions;

public interface IMemberBuilder
{
    public void AddModifiers(params SyntaxKind[] modifiers);

    public MemberDeclarationSyntax Build();
}