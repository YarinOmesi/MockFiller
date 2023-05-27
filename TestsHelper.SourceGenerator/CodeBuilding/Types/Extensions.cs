using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsHelper.SourceGenerator.CodeBuilding.Types;

public static class Extensions
{
    public static NamespacedType Type(this ITypeSymbol type) => type.ContainingNamespace.IsGlobalNamespace
        ? new NamespacedType(string.Empty, type.Name)
        : new NamespacedType(type.GetNamespace(), type.Name);

    public static string MakeString(this IType type) => type.Build().NormalizeWhitespace().ToFullString();
    public static NamespacedType Type(this string @namespace, string name) => new(@namespace, name);
    public static GenericType Generic(this NamespacedType type, params IType[] typeArguments) => new(type, typeArguments);
    public static GenericType Generic(this NamespacedType type, params ITypeSymbol[] typeArguments) => new(type, typeArguments.Select(Type).ToArray());
    public static NullableType Nullable(this IType type) => new(type);
    public static IType Type(this TypeBuilder typeBuilder) => new TypeBuilderType(typeBuilder);

    private record TypeBuilderType(TypeBuilder TypeBuilder) : IType
    {
        public string Namespace => TypeBuilder.ParentFileBuilder.Namespace;
        public string Name => TypeBuilder.Name;
        public TypeSyntax Build() => new NamespacedType(Namespace, Name).Build();
    }
}