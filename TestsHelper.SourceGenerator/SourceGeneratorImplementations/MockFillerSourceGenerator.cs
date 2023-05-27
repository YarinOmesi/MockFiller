using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsHelper.SourceGenerator.Diagnostics;
using TestsHelper.SourceGenerator.MockFilling;
using TestsHelper.SourceGenerator.MockFilling.Models;

namespace TestsHelper.SourceGenerator.SourceGeneratorImplementations;

[Generator]
public class MockFillerSourceGenerator : ISourceGenerator
{
    private static readonly MockFillerImplementation MockFillerImplementation = new();
    private static readonly TestClassMockCandidateFactory TestClassMockCandidateFactory = new();

    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Execute(GeneratorExecutionContext context)
    {
        using IDisposable _ = GlobalDiagnosticReporter.SetReporterForScope(new ActionDiagnosticReporter(context.ReportDiagnostic));

        List<TestClassMockCandidate> classesToFillMockIn = new List<TestClassMockCandidate>();

        ReportCatcher.RunCode(() =>
        {
            classesToFillMockIn = GetClassesToFillMockIn(context).ToList();
        });

        foreach (TestClassMockCandidate classToFillMockIn in classesToFillMockIn)
        {
            ReportCatcher.RunCode(() =>
            {
                foreach (FileResult result in MockFillerImplementation.Generate(classToFillMockIn))
                {
                    context.AddSource(result.FileName, result.SourceCode);
                }
            });
        }
    }

    private static IEnumerable<TestClassMockCandidate> GetClassesToFillMockIn(GeneratorExecutionContext context)
    {
        var classesToFillMockIn = new List<TestClassMockCandidate>();

        IEnumerable<SyntaxNode> allNodes = GetAllDescendantNodes(context);
        IEnumerable<ClassDeclarationSyntax> classDeclarations = GetAllClassDeclarations(allNodes);

        foreach (ClassDeclarationSyntax containingClassSyntax in classDeclarations)
        {
            SemanticModel model = context.Compilation.GetSemanticModel(containingClassSyntax.SyntaxTree);

            if (TestClassMockCandidateFactory.TryCreate(containingClassSyntax, model, out TestClassMockCandidate classToFillMockIn))
            {
                classesToFillMockIn.Add(classToFillMockIn);
            }
        }

        return classesToFillMockIn;
    }

    private static IEnumerable<SyntaxNode> GetAllDescendantNodes(GeneratorExecutionContext context)
    {
        return context.Compilation.SyntaxTrees.SelectMany(tree => tree.GetRoot().DescendantNodes());
    }

    private static IEnumerable<ClassDeclarationSyntax> GetAllClassDeclarations(IEnumerable<SyntaxNode> syntaxNodes)
    {
        return syntaxNodes
            .Where(node => node.IsKind(SyntaxKind.ClassDeclaration))
            .OfType<ClassDeclarationSyntax>();
    }
}