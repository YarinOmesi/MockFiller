using Microsoft.CodeAnalysis.Text;

namespace TestsHelper.SourceGenerator.MockFilling.Models;

public readonly record struct FileResult(string FileName, SourceText SourceCode)
{
    public string FileName { get; } = FileName;
    public SourceText SourceCode { get; } = SourceCode;
}