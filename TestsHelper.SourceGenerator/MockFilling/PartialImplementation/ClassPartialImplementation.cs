﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation;

[Obsolete("Use SyntaxTreeMockedFilledPartialClassCreator instead")]
public class ClassPartialImplementation : IMockedFilledPartialClassCreator
{
    private const string NamespaceTemplate = @"
{{NamespaceUsings}}
namespace {{Namespace}}
{
{{NamespaceBody}}        
}";

    private const string Template = @"
// <auto-generated>
{{ClassUsings}}
public partial class {{ClassName}} 
{
{{Mocks}}
}";

    private readonly HashSet<string> _usingNamespaces = new();
    private readonly List<Mock> _mocks = new();
    private string? _className;
    private string? _namespace;

    public void SetClass(ITypeSymbol typeSymbol)
    {
        _className = typeSymbol.Name;
        if (typeSymbol.ContainingNamespace.IsGlobalNamespace == false)
        {
            _namespace = GetNamespace(typeSymbol);
        }
    }

    public void SetClass(ClassDeclarationSyntax declarationSyntax)
    {
        _className = declarationSyntax.Identifier.Text;
        if (declarationSyntax.Parent is BaseNamespaceDeclarationSyntax parentNamespace)
        {
            _namespace = parentNamespace.Name.ToString();
        }
    }

    public void SetSelectedConstructor(IMethodSymbol selectedConstructor)
    {
        throw new NotImplementedException();
    }


    public void AddMockForType(ITypeSymbol typeSymbol, string name)
    {
        // TODO: Test Case The type is in global namespace (NoNamespace) 
        // TODO: Test Case The Type is in some namespace
        if (typeSymbol.ContainingNamespace.IsGlobalNamespace == false)
        {
            _usingNamespaces.Add(GetNamespace(typeSymbol));
        }

        _mocks.Add(new Mock(typeSymbol.Name, name));
        _usingNamespaces.Add("Moq");
    }

    public void AddValueForParameter(string name, string parameterName)
    {
        throw new NotImplementedException();
    }

    private string GetNamespace(ITypeSymbol symbol)
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

    private string BuildMocks()
    {
        StringBuilder mocks = new();
        foreach (Mock mock in _mocks)
        {
            mocks.Append('\t').Append('\t')
                .Append("private").Append(' ')
                .Append("Mock<").Append(mock.Type).Append(">").Append(' ')
                .Append('_').Append(mock.Name).Append("Mock")
                .Append(";").AppendLine();
        }

        return mocks.ToString();
    }

    private string BuildUsings()
    {
        if (_usingNamespaces.Contains(_namespace ?? ""))
        {
            _usingNamespaces.Remove(_namespace ?? "");
        }

        StringBuilder usings = new();
        foreach (string @namespace in _usingNamespaces)
        {
            usings
                .Append("using").Append(' ')
                .Append(@namespace)
                .Append(";").AppendLine();
        }

        return usings.ToString();
    }

    public SourceText Build()
    {
        string classSource = Template
                .Replace("{{ClassName}}", _className)
                .Replace("{{Mocks}}", BuildMocks())
            ;


        string sourceCode;
        if (string.IsNullOrEmpty(_namespace))
        {
            sourceCode = classSource
                .Replace("{{ClassUsings}}", BuildUsings())
                .Replace("{{NamespaceUsings}}", "");
        }
        else
        {
            sourceCode = NamespaceTemplate
                .Replace("{{NamespaceUsings}}", BuildUsings())
                .Replace("{{Namespace}}", _namespace)
                .Replace("{{NamespaceBody}}", classSource)
                .Replace("{{ClassUsings}}", "");
        }


        return SourceText.From(sourceCode);
    }

    private readonly record struct Mock(string Type, string Name)
    {
        public string Type { get; } = Type;
        public string Name { get; } = Name;
    }
}