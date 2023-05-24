using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsHelper.SourceGenerator.MockFilling.Models;

public readonly record struct TestClassMockCandidate(
    ClassDeclarationSyntax ContainingClassSyntax,
    INamedTypeSymbol ContainingClassSymbol,
    ITypeSymbol TestedClassMember,
    bool GenerateMockWrappers
);