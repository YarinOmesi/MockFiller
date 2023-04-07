using Microsoft.CodeAnalysis.Text;

namespace TestsHelper.SourceGenerator.MockFilling.Models;

public readonly record struct FileResult(string FileName, SourceText SourceCode);