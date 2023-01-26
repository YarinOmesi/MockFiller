using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Models;

public readonly record struct GeneratedMock(Mock Mock, FieldDeclarationSyntax FieldDeclarationSyntax)
{
    public Mock Mock { get; } = Mock;
    public FieldDeclarationSyntax FieldDeclarationSyntax { get; } = FieldDeclarationSyntax;

    public string ParameterName => Mock.ParameterName;

    public string MockVariableName => FieldDeclarationSyntax.Declaration.Variables[0].Identifier.Text;
    public TypeSyntax MockVariableType => FieldDeclarationSyntax.Declaration.Type;
}
