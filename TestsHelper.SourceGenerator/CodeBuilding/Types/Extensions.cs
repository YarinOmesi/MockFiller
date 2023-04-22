using System;
using Microsoft.CodeAnalysis;
using TestsHelper.SourceGenerator.CodeBuilding.Abstractions;

namespace TestsHelper.SourceGenerator.CodeBuilding.Types;

public static class Extensions
{
    public static NamespacedType Type(this ITypeSymbol type)
    {
        if (type.ContainingNamespace.IsGlobalNamespace)
        {
            return new NamespacedType(string.Empty, type.Name);
        }

        return new NamespacedType(type.GetNamespace(), type.Name);
    }

    public static NamespacedType Type(this string @namespace, string name) => new NamespacedType(@namespace, name);

    public static GenericType Generic(this IType type, params IType[] typeArguments)
    {
        return new GenericType(type.Namespace, type.Name, typeArguments);
    }

    public static NullableType Nullable(this IType type) => new(type);
    public static IType Type(this ITypeBuilder typeBuilder) => new TypeBuilderType(typeBuilder);

    private record TypeBuilderType(ITypeBuilder TypeBuilder) : IType
    {
        public string Namespace => TypeBuilder.ParentFileBuilder.Namespace;
        public string Name => TypeBuilder.Name;
        public void Write(IIndentedStringWriter writer) => writer.Write(Name);
    }
}