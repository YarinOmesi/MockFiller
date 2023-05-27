using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsHelper.SourceGenerator;

public static class AttributeHelpers
{
    private static string GetAttributeFullName(AttributeSyntax attributeSyntax, SemanticModel semanticModel)
    {
        TypeInfo typeInfo = semanticModel.GetTypeInfo(attributeSyntax);
        if (typeInfo.Type is IErrorTypeSymbol errorTypeSymbol)
        {
            throw new ArgumentException($"Error Get Symbol {errorTypeSymbol}");
        }

        return typeInfo.Type!.ToDisplayString();
    }

    public static Dictionary<MemberDeclarationSyntax, List<string>> MembersWithAttribute(
        this TypeDeclarationSyntax classDeclarationSyntax, SemanticModel model, string[] onlyAttributes)
    {
        Dictionary<MemberDeclarationSyntax, List<string>> dictionary = new();

        foreach (MemberDeclarationSyntax member in classDeclarationSyntax.Members)
        {
            List<string> attributes = member.AttributeLists.SelectMany(syntax => syntax.Attributes)
                .Select(syntax => GetAttributeFullName(syntax, model))
                .ToList();
            if (attributes.Any(onlyAttributes.Contains))
            {
                dictionary[member] = attributes;    
            }
        }

        return dictionary;
    }
}