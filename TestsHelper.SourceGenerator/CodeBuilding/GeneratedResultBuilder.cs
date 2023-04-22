using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using TestsHelper.SourceGenerator.CodeBuilding.Abstractions;
using TestsHelper.SourceGenerator.MockFilling.Models;

namespace TestsHelper.SourceGenerator.CodeBuilding;

public class GeneratedResultBuilder : IGeneratedResultBuilder
{
    private readonly Dictionary<string, IFileBuilder> _fileNameToType = new Dictionary<string, IFileBuilder>();
    
    public IFileBuilder CreateFileBuilder(string fileName)
    {
        var fileBuilder = new FileBuilder() {
            Name = fileName
        };
        _fileNameToType[fileName] = fileBuilder;
        return fileBuilder;
    }

    public IReadOnlyList<FileResult> GetFileResults()
    {
        List<FileResult> results = new List<FileResult>();

        foreach (string name in _fileNameToType.Keys.OrderBy(name => name))
        {
            IFileBuilder fileBuilder = _fileNameToType[name];
            var stringWriter = new StringWriter();
            var indentedStringWriter = new IndentedStringWriter(stringWriter, "    ");

            fileBuilder.Write(indentedStringWriter);

            results.Add(new FileResult(name, SourceText.From(stringWriter.ToString(), Encoding.UTF8)));

        }

        return results;
    }
}