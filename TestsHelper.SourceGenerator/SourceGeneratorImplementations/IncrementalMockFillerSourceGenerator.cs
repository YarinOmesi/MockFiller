using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsHelper.SourceGenerator.MockFilling;
using TestsHelper.SourceGenerator.MockFilling.Models;

namespace TestsHelper.SourceGenerator.SourceGeneratorImplementations;

[Generator(LanguageNames.CSharp)]
public class IncrementalMockFillerSourceGenerator : IIncrementalGenerator
{
    private static readonly MockFillerImplementation MockFillerImplementation = new();
    private static readonly TestClassMockCandidateFactory TestClassMockCandidateFactory = new();

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<TestClassMockCandidate> syntaxProvider = context.SyntaxProvider
            .CreateSyntaxProvider(Predicate, Transform)
            .WithComparer(TestClassMockCandidateEqualityComparer.Instance);

        context.RegisterSourceOutput(syntaxProvider.Collect(), Execute);
    }

    private static bool Predicate(SyntaxNode node, CancellationToken cancellationToken)
    {
        return node is ClassDeclarationSyntax classDeclarationSyntax && classDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword);
    }

    private static TestClassMockCandidate Transform(GeneratorSyntaxContext context, CancellationToken token)
    {
        ClassDeclarationSyntax declarationSyntax = (ClassDeclarationSyntax) context.Node;

        return TestClassMockCandidateFactory.Create(declarationSyntax, context.SemanticModel);
    }

    private static void Execute(SourceProductionContext context, ImmutableArray<TestClassMockCandidate> classMockCandidates)
    {
        foreach (TestClassMockCandidate testClassMockCandidate in classMockCandidates)
        {
            ReportCatcher.RunCode(() =>
            {
                foreach (FileResult result in MockFillerImplementation.Generate(testClassMockCandidate))
                {
                    context.AddSource(result.FileName, result.SourceCode);
                }
            }, context.ReportDiagnostic);
        }
    }

    private sealed class TestClassMockCandidateEqualityComparer : IEqualityComparer<TestClassMockCandidate>
    {
        public static TestClassMockCandidateEqualityComparer Instance { get; } = new TestClassMockCandidateEqualityComparer();

        public bool Equals(TestClassMockCandidate x, TestClassMockCandidate y)
        {
            return x.ContainingClassIdentifier.Equals(y.ContainingClassIdentifier) 
                   && x.ContainsClassNamespace == y.ContainsClassNamespace 
                   && SymbolEqualityComparer.Default.Equals(x.ContainingClassSymbol, y.ContainingClassSymbol)
                   && x.AttributedTestClassMembers.SequenceEqual(y.AttributedTestClassMembers);
        }

        public int GetHashCode(TestClassMockCandidate obj) => throw new NotImplementedException();
    }
}