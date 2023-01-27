using Microsoft.CodeAnalysis;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Models;

public readonly record struct Mock(ITypeSymbol Type, string ParameterName)
{
    public ITypeSymbol Type { get; } = Type;
    public string ParameterName { get; } = ParameterName;
}