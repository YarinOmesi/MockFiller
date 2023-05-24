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
    private static readonly ClassToFillMockInFactory ClassToFillMockInFactory = new();

    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Execute(GeneratorExecutionContext context)
    {
        using IDisposable _ = GlobalDiagnosticReporter.SetReporterForScope(new ActionDiagnosticReporter(context.ReportDiagnostic));

        List<ClassToFillMockIn> classesToFillMockIn = new List<ClassToFillMockIn>();

        ReportCatcher.RunCode(() =>
        {
            classesToFillMockIn = GetClassesToFillMockIn(context).ToList();
        });

        foreach (ClassToFillMockIn classToFillMockIn in classesToFillMockIn)
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

    private static IEnumerable<ClassToFillMockIn> GetClassesToFillMockIn(GeneratorExecutionContext context)
    {
        var classesToFillMockIn = new List<ClassToFillMockIn>();

        IEnumerable<SyntaxNode> allNodes = GetAllDescendantNodes(context);
        IEnumerable<ClassDeclarationSyntax> classDeclarations = GetAllClassDeclarations(allNodes);

        foreach (ClassDeclarationSyntax classDeclarationSyntax in classDeclarations)
        {
            SemanticModel model = context.Compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);

            if (ClassToFillMockInFactory.TryCreate(classDeclarationSyntax, model, out ClassToFillMockIn classToFillMockIn))
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