using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Models;

public readonly record struct GeneratedMock(Mock Mock, TypeSyntax MockVariableType, string TypeNamespace)
{
    public string ParameterName => Mock.ParameterName;
}