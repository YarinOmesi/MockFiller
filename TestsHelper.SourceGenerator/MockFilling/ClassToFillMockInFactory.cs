using System.Collections.Immutable;
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
    public bool TryCreate(ClassDeclarationSyntax declarationSyntax, SemanticModel model, out ClassToFillMockIn classToFillMockIn)
    {
        ImmutableList<MemberDeclarationSyntax> membersWithAttribute =
            declarationSyntax.GetMembersWithAttribute<FillMocksAttribute>(model);

        if (membersWithAttribute.Count == 0)
        {
            classToFillMockIn = default;
            return false;
        }

        if (membersWithAttribute.Count > 1)
        {
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

        MemberDeclarationSyntax testedClassMember = membersWithAttribute.First();
        TypeSyntax type = GetTypeFromFieldOrProperty(testedClassMember)!;

        ITypeSymbol testedClassTypeSymbol = model.GetTypeInfo(type).Type!;
        INamedTypeSymbol declarationSymbol = model.GetDeclaredSymbol(declarationSyntax)!;
        // TODO: diagnostic if there are null

        classToFillMockIn = new ClassToFillMockIn(declarationSyntax, declarationSymbol, testedClassTypeSymbol);
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