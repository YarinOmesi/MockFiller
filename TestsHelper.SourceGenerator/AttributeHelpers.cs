using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsHelper.SourceGenerator;

public static class AttributeHelpers
{
    public static bool ContainsAttribute(
        this SyntaxList<AttributeListSyntax> attributeListSyntax,
        SemanticModel semanticModel,
        string fullName)
    {
        return attributeListSyntax.Any(list => list.Attributes.Any(syntax => IsAttributeSame(syntax, semanticModel, fullName)));
    }

    private static bool IsAttributeSame(AttributeSyntax attributeSyntax, SemanticModel semanticModel, string attributeFullName)
    {
        TypeInfo typeInfo = semanticModel.GetTypeInfo(attributeSyntax);
        if (typeInfo.Type is IErrorTypeSymbol errorTypeSymbol)
        {
            throw new ArgumentException($"Error Get Symbol {errorTypeSymbol}");
        }

        string fullName = typeInfo.Type!.ToDisplayString();
        return attributeFullName == fullName;
    }

    public static ImmutableList<MemberDeclarationSyntax> GetMembersWithAttribute<T>(
        this TypeDeclarationSyntax classDeclarationSyntax,
        SemanticModel semanticModel
    ) where T : Attribute
    {
        return classDeclarationSyntax.GetMembersWithAttribute(semanticModel, typeof(T).FullName);
    }

    public static ImmutableList<MemberDeclarationSyntax> GetMembersWithAttribute(this TypeDeclarationSyntax classDeclarationSyntax,
        SemanticModel semanticModel, string attributeFullName)
    {
        return classDeclarationSyntax.Members
            .Where(member => member.AttributeLists.ContainsAttribute(semanticModel, attributeFullName))
            .ToImmutableList();
    }
}