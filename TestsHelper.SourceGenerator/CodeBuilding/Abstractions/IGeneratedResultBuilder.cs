namespace TestsHelper.SourceGenerator.CodeBuilding.Abstractions;

public interface IGeneratedResultBuilder
{
    public IFileBuilder CreateFileBuilder(string fileName);
}