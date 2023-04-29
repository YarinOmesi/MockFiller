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
    public List<SyntaxKind> SyntaxKindModifiers { get; } = new List<SyntaxKind>();

    public void AddModifiers(params SyntaxKind[] modifiers) => SyntaxKindModifiers.AddRange(modifiers);

    protected SyntaxTokenList BuildModifiers() => SyntaxFactory.TokenList(SyntaxKindModifiers.Select(SyntaxFactory.Token));

    [Pure]
    public abstract MemberDeclarationSyntax Build();
}