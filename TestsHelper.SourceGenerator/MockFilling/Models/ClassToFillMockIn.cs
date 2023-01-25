using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsHelper.SourceGenerator.MockFilling.Models;

public readonly record struct ClassToFillMockIn(
    ClassDeclarationSyntax DeclarationSyntax,
    INamedTypeSymbol DeclarationSymbol,
    ITypeSymbol TestedClassMember,
    bool GenerateMockWrappers
)
{
    public ClassDeclarationSyntax DeclarationSyntax { get; } = DeclarationSyntax;
    public ITypeSymbol TestedClassMember { get; } = TestedClassMember;
    public INamedTypeSymbol DeclarationSymbol { get; } = DeclarationSymbol;

    public bool GenerateMockWrappers { get; } = GenerateMockWrappers;
}