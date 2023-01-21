using System;
using Microsoft.CodeAnalysis;
using TestsHelper.SourceGenerator.Diagnostics.Reporters;

namespace TestsHelper.SourceGenerator.Diagnostics;

public static class GlobalDiagnosticReporter
{
    public static IDiagnosticReporter? Reporter = null;

    public static void Report(Diagnostic diagnostic) => Reporter!.Report(diagnostic);

    public static void Report(DiagnosticDescriptor descriptor, Location location, params object[] args) =>
        Reporter!.Report(Diagnostic.Create(descriptor, location, args));


    public static IDisposable SetReporterForScope(IDiagnosticReporter reporter)
    {
        Reporter = reporter;
        return new Scope(static () => Reporter = null);
    }

    private class Scope : IDisposable
    {
        private readonly Action _action;

        public Scope(Action action)
        {
            _action = action;
        }

        public void Dispose() => _action();
    }
}