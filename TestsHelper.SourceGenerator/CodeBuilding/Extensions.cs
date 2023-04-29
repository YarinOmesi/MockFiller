using Microsoft.CodeAnalysis.CSharp;
using TestsHelper.SourceGenerator.CodeBuilding.Abstractions;

namespace TestsHelper.SourceGenerator.CodeBuilding;

public static class Extensions
{
    public static T Public<T>(this T builder) where T : IMemberBuilder
    {
        builder.AddModifiers(SyntaxKind.PublicKeyword);
        return builder;
    }

    public static T Private<T>(this T builder) where T : IMemberBuilder
    {
        builder.AddModifiers(SyntaxKind.PrivateKeyword);
        return builder;
    }

    public static T Readonly<T>(this T builder) where T : IMemberBuilder
    {
        builder.AddModifiers(SyntaxKind.ReadOnlyKeyword);
        return builder;
    }

    public static T Partial<T>(this T builder) where T : IMemberBuilder
    {
        builder.AddModifiers(SyntaxKind.PartialKeyword);
        return builder;
    }

    public static ITypeBuilder AddClass(this IFileBuilder builder, string? name = null)
    {
        var classBuilder = TypeBuilder.ClassBuilder(builder);
        builder.AddTypes(classBuilder);
        if (name != null) classBuilder.Name = name;
        return classBuilder;
    }

    public static ITypeBuilder AddClass(this ITypeBuilder type)
    {
        var classBuilder = TypeBuilder.ClassBuilder(type.ParentFileBuilder);
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

    public static T Add<T>(this T builder, ITypeBuilder type) where T : IMemberBuilder
    {
        type.AddMembers(builder);
        return builder;
    }

    public static IParameterBuilder Add(this IParameterBuilder builder, IMethodLikeBuilder methodLikeBuilder)
    {
        methodLikeBuilder.AddParameters(builder);
        return builder;
    }

    public static IParameterBuilder InitializeFieldWithParameter(
        this ConstructorBuilder builder,
        FieldBuilder field,
        string parameterName
    )
    {
        IParameterBuilder parameterBuilder = ParameterBuilder.Create(field.Type, parameterName).Add(builder);
        builder.AddBodyStatements($"{field.Name} = {parameterName};");
        return parameterBuilder;
    }
}