using System.Collections.Generic;

namespace TestsHelper.SourceGenerator.CodeBuilding.Abstractions;

public interface IFileBuilder : IWritable
{
    public IReadOnlyList<ITypeBuilder> Types { get; }
    public string Namespace { get; set; }
    public string Name { get; set; }

    public void AddUsings(params string[] usings);
    public void AddTypes(params ITypeBuilder[] typeBuilders);
}