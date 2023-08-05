using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace TestsHelper.SourceGenerator.MockFilling.Models;

/// <summary>
/// 
/// </summary>
/// <param name="ContainsClassNamespace">empty when is in global namespace</param>
/// <param name="ContainingClassSymbol"></param>
/// <param name="AttributedTestClassMembers"></param>
public readonly record struct TestClassMockCandidate(
    SyntaxToken ContainingClassIdentifier,
    string ContainsClassNamespace,
    INamedTypeSymbol ContainingClassSymbol,
    ImmutableArray<AttributedTestClassMember> AttributedTestClassMembers
);