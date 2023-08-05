using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsHelper.SourceGenerator.CodeBuilding.Types;

public record AliasType(string Name, NameSyntax AliasTo) : IType
{
    public string Namespace => string.Empty;

    public virtual TypeSyntax Build() => SyntaxFactory.IdentifierName(Name);
}

public sealed record NullableAliasType(AliasType AliasType) : AliasType(AliasType)
{
    public override TypeSyntax Build() => SyntaxFactory.NullableType(AliasType.Build());
}