using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsHelper.SourceGenerator.CodeBuilding.Types;

[DebuggerDisplay("{Namespace}.{Name}<>")]
public record GenericType(NamespacedType Type, IReadOnlyList<IType> TypedArguments) : IType
{
    public string Namespace { get; } = Type.Namespace;
    public string Name { get; } = Type.Name;

    public TypeSyntax Build()
    {
        TypeSyntax typeSyntax = Type.Build();

        var typeArguments = TypedArguments.Select(type => type.Build()).ToArray();
        return typeSyntax switch {
            IdentifierNameSyntax identifierNameSyntax => SyntaxFactory.GenericName(identifierNameSyntax.Identifier)
                .AddTypeArgumentListArguments(typeArguments),
            _ => SyntaxFactory.GenericName(Name).AddTypeArgumentListArguments(typeArguments)
        };
    }
}