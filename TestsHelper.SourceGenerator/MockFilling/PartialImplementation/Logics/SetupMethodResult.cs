using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Logics;

public readonly record struct SetupMethodResult(string[] Usings, IReadOnlyList<MemberDeclarationSyntax> MemberDeclarations)
{
    public string[] Usings { get; } = Usings;
    public IReadOnlyList<MemberDeclarationSyntax> MemberDeclarations { get; } = MemberDeclarations;
}