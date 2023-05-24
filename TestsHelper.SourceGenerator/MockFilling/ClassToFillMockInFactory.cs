using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsHelper.SourceGenerator.Attributes;
using TestsHelper.SourceGenerator.Diagnostics;
using TestsHelper.SourceGenerator.Diagnostics.Exceptions;
using TestsHelper.SourceGenerator.MockFilling.Models;

namespace TestsHelper.SourceGenerator.MockFilling;

public class ClassToFillMockInFactory
{
    private const string MockWrappersAttributeFullName = "TestsHelper.SourceGenerator.MockWrapping.FillMocksWithWrappersAttribute";

    public bool TryCreate(ClassDeclarationSyntax containingClassSyntax, SemanticModel model, out ClassToFillMockIn classToFillMockIn)
    {
        string[] attributes = {MockWrappersAttributeFullName, typeof(FillMocksAttribute).FullName};

        var membersToAttributes = containingClassSyntax.MembersWithAttribute(model, attributes);

        // Check That Only One Member Have The Attribute
        switch (membersToAttributes.Count)
        {
            case 0:
                classToFillMockIn = default;
                return false;
            case > 1:
                throw new DiagnosticException(DiagnosticRegistry.MoreThanOneFillMockUsage, containingClassSyntax.Identifier.GetLocation());
        }

        // Error If Class Is Not Partial
        if (!containingClassSyntax.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            throw new DiagnosticException(
                DiagnosticRegistry.ClassIsNotPartial,
                containingClassSyntax.Identifier.GetLocation(),
                containingClassSyntax.Identifier.Text
            );
        }

        var pair = membersToAttributes.First();
        MemberDeclarationSyntax testedClassMember = pair.Key;
        bool generateMockWrappers = pair.Value.Contains(MockWrappersAttributeFullName);

        TypeSyntax type = GetTypeFromFieldOrProperty(testedClassMember)!;

        ITypeSymbol testedClassTypeSymbol = model.GetTypeInfo(type).Type!;
        INamedTypeSymbol declarationSymbol = model.GetDeclaredSymbol(containingClassSyntax)!;
        // TODO: diagnostic if there are null

        classToFillMockIn = new ClassToFillMockIn(containingClassSyntax, declarationSymbol, testedClassTypeSymbol, generateMockWrappers);
        return true;
    }

    private static TypeSyntax? GetTypeFromFieldOrProperty(MemberDeclarationSyntax member)
    {
        return member.RawKind switch {
            (int) SyntaxKind.PropertyDeclaration => ((BasePropertyDeclarationSyntax) member).Type,
            (int) SyntaxKind.FieldDeclaration => ((BaseFieldDeclarationSyntax) member).Declaration.Type,
            _ => default
        };
    }
}