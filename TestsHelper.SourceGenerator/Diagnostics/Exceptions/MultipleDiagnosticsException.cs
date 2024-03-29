﻿using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace TestsHelper.SourceGenerator.Diagnostics.Exceptions;

public class MultipleDiagnosticsException : Exception
{
    public IReadOnlyList<Diagnostic> Diagnostics { get; }

    public MultipleDiagnosticsException(IReadOnlyList<Diagnostic> diagnostics)
    {
        Diagnostics = diagnostics;
    }
}