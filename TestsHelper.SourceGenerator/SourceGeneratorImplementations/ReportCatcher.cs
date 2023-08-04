using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using TestsHelper.SourceGenerator.Diagnostics;
using TestsHelper.SourceGenerator.Diagnostics.Exceptions;

namespace TestsHelper.SourceGenerator.SourceGeneratorImplementations;

public static class ReportCatcher
{
    public static void RunCode(Action action, Action<Diagnostic> reporter)
    {
        try
        {
            action();
        }
        catch (DiagnosticException e)
        {
            reporter(e.Diagnostic);
        }
        catch (MultipleDiagnosticsException e)
        {
            foreach (Diagnostic diagnostic in e.Diagnostics)
            {
                reporter(diagnostic);
            }
        }
    }
}