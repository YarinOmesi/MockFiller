using System.Collections.Generic;
using TestsHelper.SourceGenerator.CodeBuilding.Abstractions;

namespace TestsHelper.SourceGenerator.CodeBuilding;

public class FileBuilder : IFileBuilder
{
    public string Namespace { get; set; } = null!;
    public List<string> Usings { get; } = new List<string>();
    public IReadOnlyList<ITypeBuilder> Types => _types;
    public string Name { get; set; } = null!;
    
    private readonly List<ITypeBuilder> _types = new ();
    
    public void AddUsings(params string[] usings) => Usings.AddRange(usings);

    public void AddTypes(params ITypeBuilder[] typeBuilders) => _types.AddRange(typeBuilders);

    public void Write(IIndentedStringWriter writer)
    {
        foreach (string @using in Usings)
        {
            writer.WriteLine(@using);
        }
        writer.WriteLine();

        writer.WriteLine($"namespace {Namespace}");
        
        Writer.Block.Write(writer, Types);
    }
}