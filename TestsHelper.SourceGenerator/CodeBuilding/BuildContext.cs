using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsHelper.SourceGenerator.CodeBuilding.Types;

namespace TestsHelper.SourceGenerator.CodeBuilding;

public readonly record struct BuildContext(FileBuilder FileBuilder)
{
    private readonly ISet<string> _nameAliases = new HashSet<string>();

    private bool TryRegisterAlias(IType type, [NotNullWhen(true)] out AliasType? aliasType)
    {
        if (type.TryCreateAlias(out aliasType))
        {
            if (_nameAliases.Contains(aliasType.Name))
            {
                return true;
            }

            UsingDirectiveSyntax usingDirectiveSyntax = SyntaxFactory.UsingDirective(aliasType.AliasTo)
                .WithAlias(SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName(aliasType.Name)));

            FileBuilder.AddUsing(usingDirectiveSyntax);
            _nameAliases.Add(aliasType.Name);
            return true;
        }

        return false;
    }

    public IType TryRegisterAlias(IType type) => TryRegisterAlias(type, out AliasType? aliasType) ? aliasType : type;
}