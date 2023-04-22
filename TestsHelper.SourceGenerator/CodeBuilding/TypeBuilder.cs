using System.Collections.Generic;
using TestsHelper.SourceGenerator.CodeBuilding.Abstractions;

namespace TestsHelper.SourceGenerator.CodeBuilding;

public class TypeBuilder : MemberBuilder, ITypeBuilder
{
    public IFileBuilder ParentFileBuilder { get; } 

    public string Name { get; set; } = null!;

    public string Namespace => ParentFileBuilder.Namespace;

    public IReadOnlyList<IMemberBuilder> Members => _members;

    private readonly List<IMemberBuilder> _members = new();

    private readonly string _kind;

    private TypeBuilder(string kind, IFileBuilder builder)
    {
        _kind = kind;
        ParentFileBuilder = builder;
    }

    public void AddMembers(params IMemberBuilder[] memberBuilders) => _members.AddRange(memberBuilders);

    public override void Write(IIndentedStringWriter writer)
    {
        WriteModifiers(writer);
        writer.WriteSpaceSeperated(_kind, Name);
        writer.WriteLine();

        Writer.Block.Write(writer, Members);
    }

    public static TypeBuilder ClassBuilder(IFileBuilder fileBuilder) => new("class", fileBuilder);
}