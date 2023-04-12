using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OneOf;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;


namespace TestsHelper.SourceGenerator.FluentSyntaxCreation;

public static partial class Extensions
{
    public static GenericNameSyntax Generic(this string type, params OneOf<string, TypeSyntax>[] generics)
    {
        List<TypeSyntax> types = generics
            .Select(argument => argument.Match(IdentifierName, ReturnInput))
            .ToList();

        SeparatedSyntaxList<TypeSyntax> list = types switch {
            {Count: 1} => SingletonSeparatedList(types[0]),
            _ => SeparatedList(types)
        };
        return GenericName(Identifier(type), TypeArgumentList(list));
    }
}