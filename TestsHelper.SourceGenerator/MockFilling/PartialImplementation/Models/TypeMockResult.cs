using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Models;

public readonly record struct TypeMockResult(
    string Name, 
    string Namespace,
    GeneratedMock GeneratedMock,
    PropertyDeclarationSyntax MockProperty,
    CompilationUnitSyntax CompilationUnitSyntax)
{
    public string Name { get; } = Name;
    public string Namespace { get; } = Namespace;
    public PropertyDeclarationSyntax MockProperty { get; } = MockProperty;
    public string MockPropertyName { get; } = MockProperty.Identifier.Text;
    public CompilationUnitSyntax CompilationUnitSyntax { get; } = CompilationUnitSyntax;
    public GeneratedMock GeneratedMock { get; } = GeneratedMock;

    public ITypeSymbol WrappedType => GeneratedMock.Mock.Type;

    public string ParameterName => GeneratedMock.ParameterName;
}