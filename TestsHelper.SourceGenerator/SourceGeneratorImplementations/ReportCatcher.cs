using System;
using TestsHelper.SourceGenerator.Diagnostics;
using TestsHelper.SourceGenerator.Diagnostics.Exceptions;
using TestsHelper.SourceGenerator.Diagnostics.Reporters;

namespace TestsHelper.SourceGenerator.SourceGeneratorImplementations;

public static class ReportCatcher
{
    public static void RunCode(Action action)
    {
        IDiagnosticReporter reporter = GlobalDiagnosticReporter.Reporter!;
        try
        {
            action();
        }
        catch (DiagnosticException e)
        {
            reporter.Report(e.Diagnostic);
        }
        catch (MultipleDiagnosticsException e)
        {
            reporter.ReportMultiple(e.Diagnostics);
        }
    }
}