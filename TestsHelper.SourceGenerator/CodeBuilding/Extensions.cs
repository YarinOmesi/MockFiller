using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsHelper.SourceGenerator.CodeBuilding.Types;
using TestsHelper.SourceGenerator.MockFilling;

namespace TestsHelper.SourceGenerator.CodeBuilding;

public static class Extensions
{
    public static T Partial<T>(this T builder) where T : MemberBuilder => builder.AddModifiers<T>(SyntaxKind.PartialKeyword);
    public static T Readonly<T>(this T builder) where T : MemberBuilder => builder.AddModifiers<T>(SyntaxKind.ReadOnlyKeyword);
    public static T Private<T>(this T builder) where T : MemberBuilder => builder.AddModifiers<T>(SyntaxKind.PrivateKeyword);
    public static T Public<T>(this T builder) where T : MemberBuilder => builder.AddModifiers<T>(SyntaxKind.PublicKeyword);

    public static T AddModifiers<T>(this T builder, params SyntaxKind[] modifiers) where T : MemberBuilder
    {
        builder.AddModifiers(modifiers);
        return builder;
    }

    public static TypeBuilder AddClass(this FileBuilder builder, string? name = null)
    {
        var classBuilder = TypeBuilder.ClassBuilder(builder);
        builder.AddTypes(classBuilder);
        if (name != null) classBuilder.Name = name;
        return classBuilder;
    }

    public static TypeBuilder AddClass(this TypeBuilder type)
    {
        var classBuilder = TypeBuilder.ClassBuilder(type);
        type.AddMembers(classBuilder);
        return classBuilder;
    }

    public static ConstructorBuilder InitializeFieldWithParameters(
        this ConstructorBuilder builder,
        params (FieldBuilder, string)[] fieldsToInitialize
    )
    {
        foreach ((FieldBuilder fieldBuilder, string parameterName) in fieldsToInitialize)
        {
            builder.InitializeFieldWithParameter(fieldBuilder, parameterName);
        }

        return builder;
    }

    public static T Add<T>(this T builder, TypeBuilder type) where T : MemberBuilder
    {
        type.AddMembers(builder);
        return builder;
    }

    public static ParameterBuilder InitializeFieldWithParameter(
        this ConstructorBuilder builder,
        FieldBuilder field,
        string parameterName
    )
    {
        ParameterBuilder parameterBuilder = ParameterBuilder.Create(field.Type, parameterName);
        builder.AddParameters(parameterBuilder);
        builder.AddBodyStatements(field.Assign(parameterName));
        return parameterBuilder;
    }

    private static readonly Dictionary<string, ISet<string>> _fileNameToAliasName = new();

    public static bool TryRegisterAlias(this IType type, FileBuilder builder, [NotNullWhen(true)] out AliasType? aliasType)
    {
        if (!_fileNameToAliasName.TryGetValue(builder.Name, out var names))
        {
            _fileNameToAliasName[builder.Name] = names = new HashSet<string>();
        }

        if (type.TryCreateAlias(out aliasType))
        {
            if (names.Contains(aliasType.Name))
            {
                return true;
            }
            UsingDirectiveSyntax usingDirectiveSyntax = SyntaxFactory.UsingDirective(aliasType.AliasTo)
                .WithAlias(SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName(aliasType.Name)));

            builder.AddUsing(usingDirectiveSyntax);
            names.Add(aliasType.Name);
            return true;
        }

        return false;
    }

    public static IType TryRegisterAlias(this IType type, FileBuilder builder)
    {
        return type.TryRegisterAlias(builder, out AliasType? aliasType) ? aliasType : type;
    }
}