using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Models;

public readonly record struct TypeMockResult(
    string Name, 
    string Namespace,
    GeneratedMock GeneratedMock,
    string MockPropertyName,
    CompilationUnitSyntax CompilationUnitSyntax)
{
    public ITypeSymbol WrappedType => GeneratedMock.Mock.Type;

    public string ParameterName => GeneratedMock.ParameterName;
}