using System.Collections.Generic;

namespace TestsHelper.SourceGenerator.CodeBuilding.Abstractions;

public interface ITypeBuilder : IMemberBuilder
{
    public IReadOnlyList<IMemberBuilder> Members { get; }
    public IFileBuilder ParentFileBuilder { get; }
    public string Name { get; set; }
    public void AddMembers(params IMemberBuilder[] memberBuilders);
}