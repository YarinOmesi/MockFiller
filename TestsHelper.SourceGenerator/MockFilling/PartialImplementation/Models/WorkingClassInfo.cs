using Microsoft.CodeAnalysis;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Models;

public readonly record struct WorkingClassInfo(string Namespace, string Name,IMethodSymbol SelectedConstructor)
{
    public string Namespace { get; } = Namespace;
    public string Name { get; } = Name;
    public IMethodSymbol SelectedConstructor { get; } = SelectedConstructor;
}