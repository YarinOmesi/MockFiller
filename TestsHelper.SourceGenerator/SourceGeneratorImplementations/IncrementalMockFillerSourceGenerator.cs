using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsHelper.SourceGenerator.Diagnostics;
using TestsHelper.SourceGenerator.Diagnostics.Exceptions;
using TestsHelper.SourceGenerator.Diagnostics.Reporters;
using TestsHelper.SourceGenerator.MockFilling;
using TestsHelper.SourceGenerator.MockFilling.Models;

namespace TestsHelper.SourceGenerator.SourceGeneratorImplementations;

public class IncrementalMockFillerSourceGenerator : IIncrementalGenerator
{
    private static readonly MockFillerImplementation MockFillerImplementation = new();
    private static readonly ClassToFillMockInFactory ClassToFillMockInFactory = new();

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<ResultClass> syntaxProvider = context.SyntaxProvider
            .CreateSyntaxProvider(Predicate, Transform);

        context.RegisterSourceOutput(syntaxProvider.Collect(), Execute);
    }

    private static bool Predicate(SyntaxNode node, CancellationToken cancellationToken)
    {
        return node is ClassDeclarationSyntax;
    }

    private static ResultClass Transform(GeneratorSyntaxContext context, CancellationToken token)
    {
        ClassDeclarationSyntax declarationSyntax = (ClassDeclarationSyntax) context.Node;

        var reporter = new MemoryDiagnosticReporter();
        using IDisposable _ = GlobalDiagnosticReporter.SetReporterForScope(reporter);
        try
        {
            if (ClassToFillMockInFactory.TryCreate(declarationSyntax, context.SemanticModel, out ClassToFillMockIn classToFillMockIn))
            {
                return new ResultClass(reporter.Diagnostics, classToFillMockIn);
            }
        }
        catch (DiagnosticException e)
        {
            reporter.Report(e.Diagnostic);
            return new ResultClass(reporter.Diagnostics);
        }
        catch (MultipleDiagnosticsException e)
        {
            reporter.ReportMultiple(e.Diagnostics);
            return new ResultClass(reporter.Diagnostics);
        }

        return new ResultClass(reporter.Diagnostics);
    }

    private static void Execute(SourceProductionContext context, ImmutableArray<ResultClass> classesToFill)
    {
        var reporter = new ActionDiagnosticReporter(context.ReportDiagnostic);
        using IDisposable _ = GlobalDiagnosticReporter.SetReporterForScope(reporter);

        foreach (ResultClass resultClass in classesToFill)
        {
            // Report All Diagnostics From First Phase
            reporter.ReportMultiple(resultClass.Diagnostics);
            
            // Continue if there is no class to fill mock in
            if(resultClass.ClassToFillMockIn is not { } classToFillMockIn)
                continue;

            ReportCatcher.RunCode(() =>
            {
                foreach (FileResult result in MockFillerImplementation.Generate(classToFillMockIn))
                {
                    context.AddSource(result.FileName, result.SourceCode);
                }
            });
        }
    }

    private readonly record struct ResultClass(IReadOnlyList<Diagnostic> Diagnostics, ClassToFillMockIn? ClassToFillMockIn = null);
}