using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Models;

public readonly record struct GeneratedMock(Mock Mock, TypeSyntax MockVariableType, string TypeNamespace)
{
    public Mock Mock { get; } = Mock;

    public string ParameterName => Mock.ParameterName;

    public TypeSyntax MockVariableType { get; } = MockVariableType;
    public string TypeNamespace { get; } = TypeNamespace;
}