using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsHelper.SourceGenerator.CodeBuilding.Types;

[DebuggerDisplay("{Namespace}.{Name}`{TypedArguments.Count}")]
public record RegularType(string Namespace, string Name, IReadOnlyList<IType> TypedArguments) : IType<QualifiedNameSyntax>
{
    private static readonly IReadOnlyList<IType> EmptyList = new List<IType>();

    public RegularType(string Namespace, string Name) : this(Namespace, Name, EmptyList)
    {
    }

    TypeSyntax IType.Build() => Build();

    public QualifiedNameSyntax Build()
    {
        SimpleNameSyntax nameSyntax = TypedArguments.Count switch {
            0 => SyntaxFactory.IdentifierName(Name),
            _ => SyntaxFactory.GenericName(Name).AddTypeArgumentListArguments(TypedArguments.Select(type => type.Build()).ToArray())
        };

        return SyntaxFactory.QualifiedName(SyntaxFactory.ParseName($"global::{Namespace}"), nameSyntax);
    }
}