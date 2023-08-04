using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsHelper.SourceGenerator.Attributes;
using TestsHelper.SourceGenerator.MockFilling.Models;

namespace TestsHelper.SourceGenerator.MockFilling;

public class TestClassMockCandidateFactory
{
    private const string MockWrappersAttributeFullName = "TestsHelper.SourceGenerator.MockWrapping.FillMocksWithWrappersAttribute";

    public TestClassMockCandidate Create(ClassDeclarationSyntax containingClassSyntax, SemanticModel model)
    {
        string[] attributes = {MockWrappersAttributeFullName, typeof(FillMocksAttribute).FullName};

        var membersToAttributes = containingClassSyntax.MembersWithAttribute(model, attributes);

        Debug.Assert(containingClassSyntax.Modifiers.Any(SyntaxKind.PartialKeyword));

        ImmutableArray<AttributedTestClassMember> attributedTestClassMembers = membersToAttributes.Select(pair =>
        {
            MemberDeclarationSyntax testedClassMember = pair.Key;
            bool generateMockWrappers = pair.Value.Contains(MockWrappersAttributeFullName);
            TypeSyntax type = GetTypeFromFieldOrProperty(testedClassMember)!;
            ITypeSymbol testedClassTypeSymbol = model.GetTypeInfo(type).Type!;

            return new AttributedTestClassMember(testedClassTypeSymbol, generateMockWrappers);
        }).ToImmutableArray();

        INamedTypeSymbol declarationSymbol = model.GetDeclaredSymbol(containingClassSyntax)!;

        return new TestClassMockCandidate(
            containingClassSyntax.Identifier,
            declarationSymbol.GetNamespace(),
            declarationSymbol,
            attributedTestClassMembers
        );
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