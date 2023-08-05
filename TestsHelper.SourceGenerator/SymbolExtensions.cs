using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace TestsHelper.SourceGenerator;

public static class SymbolExtensions
{
    public static string GetNamespace(this ITypeSymbol symbol)
    {
        List<string> namespaces = new();

        INamespaceSymbol @namespace = symbol.ContainingNamespace;
        while (@namespace.IsGlobalNamespace == false)
        {
            namespaces.Add(@namespace.Name);
            @namespace = @namespace.ContainingNamespace;
        }

        namespaces.Reverse();
        return string.Join(".", namespaces);
    }
}