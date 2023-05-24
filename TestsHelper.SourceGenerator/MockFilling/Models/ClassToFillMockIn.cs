using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsHelper.SourceGenerator.MockFilling.Models;

public readonly record struct ClassToFillMockIn(
    ClassDeclarationSyntax ContainingClassSyntax,
    INamedTypeSymbol DeclarationSymbol,
    ITypeSymbol TestedClassMember,
    bool GenerateMockWrappers
);