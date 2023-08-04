using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsHelper.SourceGenerator.CodeBuilding.Types;

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

    public void AddUsingFor(IType type) => AddUsings(type.Namespace);

    public void AddUsings(params string[] usings) =>
        _usings.AddRange(usings.Select(s => SyntaxFactory.ParseName(s)).Select(SyntaxFactory.UsingDirective));

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