using Microsoft.CodeAnalysis.Text;

namespace MockFiller.Analyzers.SourceGenerators.MockFilling.Models;

public readonly record struct FileResult(string FileName, SourceText SourceCode)
{
    public string FileName { get; } = FileName;
    public SourceText SourceCode { get; } = SourceCode;
}