using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsHelper.SourceGenerator.CodeBuilding.Abstractions;

namespace TestsHelper.SourceGenerator.CodeBuilding;

public abstract class MemberBuilder : IMemberBuilder
{
    private List<SyntaxToken> SyntaxKindModifiers { get; } = new List<SyntaxToken>();

    public void AddModifiers(params SyntaxKind[] modifiers) => SyntaxKindModifiers.AddRange(modifiers.Select(SyntaxFactory.Token));

    protected SyntaxTokenList BuildModifiers() => SyntaxFactory.TokenList(SyntaxKindModifiers);

    [Pure]
    public abstract MemberDeclarationSyntax Build();
}