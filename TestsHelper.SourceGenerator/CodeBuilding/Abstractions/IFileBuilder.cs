using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsHelper.SourceGenerator.CodeBuilding.Abstractions;

public interface IFileBuilder
{
    public IReadOnlyList<ITypeBuilder> Types { get; }
    public string Namespace { get; set; }
    public string Name { get; set; }

    public void AddUsings(params string[] usings);
    public void AddTypes(params ITypeBuilder[] typeBuilders);

    public CompilationUnitSyntax Build();
}