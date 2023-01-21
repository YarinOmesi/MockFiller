using System;
using Microsoft.CodeAnalysis;

namespace TestsHelper.SourceGenerator.Diagnostics;

public class DiagnosticException : Exception
{
    public Diagnostic Diagnostic { get; }

    public DiagnosticException(
        DiagnosticDescriptor descriptor,
        Location? location = null,
        params object?[]? messageArgs)
    {
        Diagnostic = Diagnostic.Create(descriptor, location, messageArgs);
    }
}