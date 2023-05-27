using System;
using Microsoft.CodeAnalysis;
using TestsHelper.SourceGenerator.Diagnostics.Reporters;

namespace TestsHelper.SourceGenerator.SourceGeneratorImplementations;

public class ActionDiagnosticReporter : IDiagnosticReporter
{
    private readonly Action<Diagnostic> _reporter;

    public ActionDiagnosticReporter(Action<Diagnostic> reporter)
    {
        _reporter = reporter;
    }

    public void Report(Diagnostic diagnostic) => _reporter(diagnostic);
}