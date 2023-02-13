using Microsoft.CodeAnalysis;

namespace MockFiller.Analyzers.Diagnostics.Reporters;

public interface IDiagnosticReporter
{
    public void Report(Diagnostic diagnostic);
}