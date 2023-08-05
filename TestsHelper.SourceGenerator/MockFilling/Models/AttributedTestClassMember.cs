using Microsoft.CodeAnalysis;

namespace TestsHelper.SourceGenerator.MockFilling.Models;

public readonly record struct AttributedTestClassMember(ITypeSymbol Symbol, bool GenerateMockWrapper);