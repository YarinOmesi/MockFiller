using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsHelper.SourceGenerator.CodeBuilding.Types;

public static class Extensions
{
    public static RegularType Type(this ITypeSymbol type) => type.ContainingNamespace.IsGlobalNamespace
        ? new RegularType(string.Empty, type.Name)
        : new RegularType(type.GetNamespace(), type.Name);

    public static string MakeString(this IType type) => type.Build().NormalizeWhitespace().ToFullString();
    public static RegularType Type(this string @namespace, string name) => new(@namespace, name);
    public static RegularType Generic(this RegularType type, params IType[] typeArguments) => type with {TypedArguments = typeArguments};
    public static RegularType Generic(this RegularType type, params ITypeSymbol[] typeArguments) =>type with {TypedArguments = typeArguments.Select(Type).ToArray()};
    public static NullableType Nullable(this IType type) => new(type);
    public static IType Type(this TypeBuilder typeBuilder) => new TypeBuilderType(typeBuilder);

    private static string QualifiedName(this TypeBuilder typeBuilder)
    {
        StringBuilder nameBuilder = new();

        nameBuilder.Append(typeBuilder.Name);

        TypeBuilder? currentType = typeBuilder.ParentType;
        while (currentType != null)
        {
            nameBuilder.Insert(0, '.');
            nameBuilder.Insert(0, currentType.Name);
            currentType = currentType.ParentType;
        }

        return nameBuilder.ToString();
    }

    private static string GetName(this IType type)
    {
        switch (type)
        {
            case VoidType:
                return "Void";
            case NullableType nullableType:
                return nullableType.Type.GetName();
            case RegularType regularType:
                StringBuilder nameBuilder = new StringBuilder();
                nameBuilder.Append(regularType.Name.Replace('.', '_'));
                foreach (IType typedArgument in regularType.TypedArguments)
                {
                    nameBuilder.Append('_').Append(typedArgument.GetName());
                }

                return nameBuilder.ToString();
            case TypeBuilderType typeBuilderType:
                return typeBuilderType.RegularType.GetName();
            default:
                throw new NotSupportedException();
        }
    }

    public static bool TryCreateAlias(this IType type, [NotNullWhen(true)] out AliasType? alias)
    {
        alias = default;
        switch (type)
        {
            case AliasType or VoidType:
                return false;
            case NullableType nullableType when nullableType.Type.TryCreateAlias(out AliasType? innerAlias):
                alias = new NullableAliasType(innerAlias);
                return true;
            case NullableType:
                return false;
            case RegularType regularType:
                alias = new AliasType(regularType.GetName(), regularType.Build());
                return true;
            case TypeBuilderType typeBuilderType:
                return typeBuilderType.RegularType.TryCreateAlias(out alias);
            default:
                throw new NotSupportedException();
        }
    }

    private sealed record TypeBuilderType(TypeBuilder TypeBuilder) : IType<NameSyntax>
    {
        public string Namespace => TypeBuilder.ParentFileBuilder.Namespace;
        public string Name => TypeBuilder.QualifiedName();
        public RegularType RegularType => new(Namespace, Name);
        public NameSyntax Build() => RegularType.Build();
        TypeSyntax IType.Build() => Build();
    }
}