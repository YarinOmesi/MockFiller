using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsHelper.SourceGenerator.Attributes;
using TestsHelper.SourceGenerator.Diagnostics;
using TestsHelper.SourceGenerator.MockFilling.Models;

namespace TestsHelper.SourceGenerator.MockFilling;

public class ClassToFillMockInFactory
{
    private const string MockWrappersAttributeFullName = "TestsHelper.SourceGenerator.MockWrapping.FillMocksWithWrappersAttribute";

    public bool TryCreate(ClassDeclarationSyntax declarationSyntax, SemanticModel model, out ClassToFillMockIn classToFillMockIn)
    {
        Dictionary<MemberDeclarationSyntax, bool> membersToGenerateWrappers = new();
        
        AddMembers(
            membersToGenerateWrappers,
            declarationSyntax.GetMembersWithAttribute<FillMocksAttribute>(model),
            generateWrappers: false
        );
        
        AddMembers(
            membersToGenerateWrappers,
            declarationSyntax.GetMembersWithAttribute(model, MockWrappersAttributeFullName),
            generateWrappers: true
        );

        // Check That Only One Member Have The Attribute
        switch (membersToGenerateWrappers.Count)
        {
            case 0:
                classToFillMockIn = default;
                return false;
            case > 1:
                throw new DiagnosticException(DiagnosticRegistry.MoreThanOneFillMockUsage, declarationSyntax.Identifier.GetLocation());
        }

        // Error If Class Is Not Partial
        if (!declarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            throw new DiagnosticException(
                DiagnosticRegistry.ClassIsNotPartial,
                declarationSyntax.Identifier.GetLocation(),
                declarationSyntax.Identifier.Text
            );
        }

        var pair = membersToGenerateWrappers.First();
        MemberDeclarationSyntax testedClassMember = pair.Key;
        bool generateMockWrappers = pair.Value;
        
        TypeSyntax type = GetTypeFromFieldOrProperty(testedClassMember)!;

        ITypeSymbol testedClassTypeSymbol = model.GetTypeInfo(type).Type!;
        INamedTypeSymbol declarationSymbol = model.GetDeclaredSymbol(declarationSyntax)!;
        // TODO: diagnostic if there are null

        classToFillMockIn = new ClassToFillMockIn(declarationSyntax, declarationSymbol, testedClassTypeSymbol, generateMockWrappers);
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

    private static void AddMembers(
        Dictionary<MemberDeclarationSyntax, bool> dictionary, 
        IEnumerable<MemberDeclarationSyntax> members,
        bool generateWrappers)
    {
        foreach (MemberDeclarationSyntax member in members)
        {
            dictionary[member] = generateWrappers;
        }
    }
}