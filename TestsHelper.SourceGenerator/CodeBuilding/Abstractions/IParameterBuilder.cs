using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsHelper.SourceGenerator.CodeBuilding.Types;

namespace TestsHelper.SourceGenerator.CodeBuilding.Abstractions;

public interface IParameterBuilder
{
    public string Name { get; set; }
    public IType Type { get; set; }
    public string? Initializer { get; set; }
    ParameterSyntax Build();
}