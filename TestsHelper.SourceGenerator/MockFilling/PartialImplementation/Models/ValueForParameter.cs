namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Models;

public readonly record struct ValueForParameter(string Name, string ParameterName)
{
    public string Name { get; } = Name;
    public string ParameterName { get; } = ParameterName;
}