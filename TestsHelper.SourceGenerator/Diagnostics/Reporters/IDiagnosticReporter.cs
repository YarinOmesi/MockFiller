using Microsoft.CodeAnalysis;

namespace TestsHelper.SourceGenerator.Diagnostics.Reporters;

public interface IDiagnosticReporter
{
    public void Report(Diagnostic diagnostic);
}