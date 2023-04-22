namespace TestsHelper.SourceGenerator.CodeBuilding.Abstractions;

public interface IMemberBuilder : IWritable
{
    public void AddModifiers(params string[] modifiers);
}