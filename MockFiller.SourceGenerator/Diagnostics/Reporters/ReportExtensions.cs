using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace MockFiller.SourceGenerator.Diagnostics.Reporters;

public static class ReportExtensions
{
    public static void ReportMultiple(this IDiagnosticReporter reporter, IEnumerable<Diagnostic> diagnostics)
    {
        foreach (Diagnostic diagnostic in diagnostics)
        {
            reporter.Report(diagnostic);
        }
    }
}