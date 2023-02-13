using Microsoft.CodeAnalysis;

namespace MockFiller.SourceGenerator.Diagnostics.Reporters;

public interface IDiagnosticReporter
{
    public void Report(Diagnostic diagnostic);
}