using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsHelper.SourceGenerator.FluentSyntaxCreation;
using TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Models;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Logics;

public class MockGenerator
{
    private static readonly SyntaxToken SemicolonToken = Token(SyntaxKind.SemicolonToken);

    public GeneratedMock Generate(Mock mock) => new(mock, CreateMockField(mock));

    private FieldDeclarationSyntax CreateMockField(Mock mock)
    {
        GenericNameSyntax type = "Mock".Generic(mock.Type.Name);
        
        VariableDeclarationSyntax declaration = type.DeclareVariable(VariableDeclarator($"_{mock.ParameterName}Mock"));

        return FieldDeclaration(declaration)
            .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)))
            .WithSemicolonToken(SemicolonToken);
    }
}