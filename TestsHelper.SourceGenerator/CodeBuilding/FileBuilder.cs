using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsHelper.SourceGenerator.CodeBuilding;

public class FileBuilder
{
    public string Namespace { get; set; } = null!;
    public IReadOnlyList<TypeBuilder> Types => _types;
    public string Name { get; set; }

    private readonly List<TypeBuilder> _types = new();
    private readonly List<UsingDirectiveSyntax> _usings = new();

    private FileBuilder(string name)
    {
        Name = name;
    }

    public void AddUsing(UsingDirectiveSyntax @using) => _usings.Add(@using);

    public void AddTypes(params TypeBuilder[] typeBuilders) => _types.AddRange(typeBuilders);

    public CompilationUnitSyntax Build()
    {
        BuildContext context = new BuildContext(this);
        
        var namespaceSyntax = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(Namespace))
            .AddMembers(_types.Select(builder => builder.Build(context)).ToArray());

        return SyntaxFactory.CompilationUnit()
            .AddUsings(_usings.ToArray())
            .AddMembers(namespaceSyntax);
    }

    [Pure]
    public static FileBuilder Create(string name) => new(name);
}