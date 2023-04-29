using Microsoft.CodeAnalysis.CSharp;
using TestsHelper.SourceGenerator.MockFilling;

namespace TestsHelper.SourceGenerator.CodeBuilding;

public static class Extensions
{
    public static T Public<T>(this T builder) where T : MemberBuilder
    {
        builder.AddModifiers(SyntaxKind.PublicKeyword);
        return builder;
    }

    public static T Private<T>(this T builder) where T : MemberBuilder
    {
        builder.AddModifiers(SyntaxKind.PrivateKeyword);
        return builder;
    }

    public static T Readonly<T>(this T builder) where T : MemberBuilder
    {
        builder.AddModifiers(SyntaxKind.ReadOnlyKeyword);
        return builder;
    }

    public static T Partial<T>(this T builder) where T : MemberBuilder
    {
        builder.AddModifiers(SyntaxKind.PartialKeyword);
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

    public static T Add<T>(this T builder, TypeBuilder type) where T : MemberBuilder
    {
        type.AddMembers(builder);
        return builder;
    }

    public static ParameterBuilder Add(this ParameterBuilder builder, MethodLikeBuilder methodLikeBuilder)
    {
        methodLikeBuilder.AddParameters(builder);
        return builder;
    }

    public static ParameterBuilder InitializeFieldWithParameter(
        this ConstructorBuilder builder,
        FieldBuilder field,
        string parameterName
    )
    {
        ParameterBuilder parameterBuilder = ParameterBuilder.Create(field.Type, parameterName).Add(builder);
        builder.AddBodyStatements(field.Assign(parameterName));
        return parameterBuilder;
    }
}