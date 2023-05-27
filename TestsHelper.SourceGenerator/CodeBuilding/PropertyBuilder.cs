using System.Diagnostics.Contracts;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsHelper.SourceGenerator.CodeBuilding.Types;

namespace TestsHelper.SourceGenerator.CodeBuilding;

internal class PropertyBuilder : FieldBuilder
{
    public bool AutoGetter { get; init; } = true;

    public bool AutoSetter { get; init; } = true;

    private PropertyBuilder() { }

    private static PropertyDeclarationSyntax AddAccessor(PropertyDeclarationSyntax propertySyntax, SyntaxKind kind) =>
        propertySyntax.AddAccessorListAccessors(SyntaxFactory.AccessorDeclaration(kind)
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));

    public override MemberDeclarationSyntax Build()
    {
        PropertyDeclarationSyntax syntax = SyntaxFactory.PropertyDeclaration(Type.Build(), Name)
            .WithModifiers(BuildModifiers())
            .WithInitializer(BuildInitializer());

        if (AutoSetter)
            syntax = AddAccessor(syntax, SyntaxKind.SetAccessorDeclaration);
        if (AutoGetter)
            syntax = AddAccessor(syntax, SyntaxKind.GetAccessorDeclaration);

        return syntax;
    }

    [Pure]
    public static PropertyBuilder Create(IType type, string name, string? initializer = null, bool autoGetter = false,
        bool autoSetter = false)
    {
        return new PropertyBuilder() {
            Type = type, Name = name, Initializer = initializer, AutoGetter = autoGetter, AutoSetter = autoSetter
        };
    }
}