using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace TestsHelper.SourceGenerator.Diagnostics.Reporters;

public class MemoryDiagnosticReporter : IDiagnosticReporter
{
    public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics;

    private readonly List<Diagnostic> _diagnostics;

    public MemoryDiagnosticReporter(List<Diagnostic> diagnostics)
    {
        _diagnostics = diagnostics;
    }

    public MemoryDiagnosticReporter()
    {
        _diagnostics = new List<Diagnostic>();
    }

    public void Report(Diagnostic diagnostic) => _diagnostics.Add(diagnostic);
}