using System.Linq;
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

    private sealed record TypeBuilderType(TypeBuilder TypeBuilder) : IType<SimpleNameSyntax>
    {
        public string Namespace => TypeBuilder.ParentFileBuilder.Namespace;
        public string Name => TypeBuilder.Name;
        public RegularType RegularType => new(Namespace, Name);
        public SimpleNameSyntax Build() => RegularType.Build();
        TypeSyntax IType.Build() => Build();
    }
}