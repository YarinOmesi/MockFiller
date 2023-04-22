namespace TestsHelper.SourceGenerator.CodeBuilding.Abstractions;

public interface IPropertyBuilder : IFieldBuilder
{
    public bool AutoGetter { get; set; }
    public bool AutoSetter { get; set; }
}