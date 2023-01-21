using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsHelper.SourceGenerator;

public static class AttributeHelpers
{
    public static bool ContainsAttribute<T>(this SyntaxList<AttributeListSyntax> attributeListSyntax, SemanticModel semanticModel)
        where T : Attribute
    {
        return attributeListSyntax.Any(list => list.Attributes.Any(syntax => IsAttributeSame<T>(syntax, semanticModel)));
    }

    private static bool IsAttributeSame<T>(AttributeSyntax attributeSyntax, SemanticModel semanticModel) where T : Attribute
    {
        TypeInfo typeInfo = semanticModel.GetTypeInfo(attributeSyntax);
        if (typeInfo.Type is IErrorTypeSymbol errorTypeSymbol)
        {
            throw new ArgumentException($"Error Get Symbol {errorTypeSymbol}");
        }

        string fullName = typeInfo.Type!.ToDisplayString();
        return typeof(T).FullName == fullName;
    }

    public static ImmutableList<MemberDeclarationSyntax> GetMembersWithAttribute<T>(this TypeDeclarationSyntax classDeclarationSyntax,
        SemanticModel semanticModel)
        where T : Attribute
    {
        return classDeclarationSyntax.Members
            .Where(member => member.AttributeLists.ContainsAttribute<T>(semanticModel))
            .ToImmutableList();
    }
}