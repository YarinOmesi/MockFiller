using System.Diagnostics.Contracts;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsHelper.SourceGenerator.CodeBuilding.Types;

namespace TestsHelper.SourceGenerator.CodeBuilding;

internal class PropertyBuilder : FieldBuilder
{
    public bool AutoGetter { get; set; } = true;

    public bool AutoSetter { get; set; } = true;

    private PropertyBuilder() { }

    public override MemberDeclarationSyntax Build()
    {
        PropertyDeclarationSyntax syntax = SyntaxFactory.PropertyDeclaration(Type.Build(), Name)
            .WithModifiers(BuildModifiers())
            .WithInitializer(BuildInitializer());

        if (AutoSetter)
            syntax = syntax.AddAccessorListAccessors(SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
        if(AutoGetter)
            syntax = syntax.AddAccessorListAccessors(SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));

        return syntax;
    }

    [Pure]
    public static PropertyBuilder Create(IType type, string name, string? initializer = null, bool autoGetter = false, bool autoSetter = false)
    {
        return new PropertyBuilder()
        {
            Type = type, Name = name, Initializer = initializer, AutoGetter = autoGetter, AutoSetter = autoSetter
        };
    }
}