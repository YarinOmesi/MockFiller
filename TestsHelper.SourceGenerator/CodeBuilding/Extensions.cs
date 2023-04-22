using System;
using TestsHelper.SourceGenerator.CodeBuilding.Abstractions;
using TestsHelper.SourceGenerator.CodeBuilding.Types;

namespace TestsHelper.SourceGenerator.CodeBuilding;

public static class Extensions
{
    public static ITypeBuilder AddClass(this IFileBuilder builder)
    {
        var classBuilder = TypeBuilder.ClassBuilder(builder);
        builder.AddTypes(classBuilder);
        return classBuilder;
    }

    public static ITypeBuilder AddClass(this IFileBuilder builder, string name)
    {
        ITypeBuilder typeBuilder = builder.AddClass();
        typeBuilder.Name = name;
        return typeBuilder;
    }

    public static IFieldBuilder AddField(this ITypeBuilder type, Action<IFieldBuilder> builder)
    {
        var fieldBuilder = new FieldBuilder();
        type.AddMembers(fieldBuilder);
        builder(fieldBuilder);
        return fieldBuilder;
    }

    public static IPropertyBuilder AddProperty(this ITypeBuilder type, Action<IPropertyBuilder> builder)
    {
        var propertyBuilder = new PropertyBuilder();
        type.AddMembers(propertyBuilder);
        builder(propertyBuilder);
        return propertyBuilder;
    }

    public static IPropertyBuilder AddProperty(this ITypeBuilder builder, IType type, string name)
    {
        return builder.AddProperty(propertyBuilder =>
        {
            propertyBuilder.Type = type;
            propertyBuilder.Name = name;
        });
    }

    public static ITypeBuilder AddClass(this ITypeBuilder type, Action<ITypeBuilder> builder)
    {
        var classBuilder = TypeBuilder.ClassBuilder(type.ParentFileBuilder);
        type.AddMembers(classBuilder);
        builder(classBuilder);
        return classBuilder;
    }

    public static IMethodBuilder AddMethod(this ITypeBuilder type, Action<IMethodBuilder> builder)
    {
        var methodBuilder = new MethodBuilder();
        type.AddMembers(methodBuilder);
        builder(methodBuilder);
        return methodBuilder;
    }

    public static IFieldBuilder AddField(this ITypeBuilder typeBuilder, IType type, string name) =>
        typeBuilder.AddField(builder =>
        {
            builder.Type = type;
            builder.Name = name;
        });

    public static IConstructorBuilder AddConstructor(this ITypeBuilder type, Action<IConstructorBuilder> builder)
    {
        var constructorBuilder = new ConstructorBuilder(type);
        type.AddMembers(constructorBuilder);
        builder(constructorBuilder);
        return constructorBuilder;
    }

    public static IConstructorBuilder AddConstructor(this ITypeBuilder type, params (IFieldBuilder, string)[] fieldsToInitialize)
    {
        return type.AddConstructor(builder =>
        {
            foreach ((IFieldBuilder fieldBuilder, string parameterName) in fieldsToInitialize)
            {
                builder.InitializeFieldWithParameter(fieldBuilder, parameterName);
            }
        });
    }

    public static IParameterBuilder InitializeFieldWithParameter(
        this IConstructorBuilder constructorBuilder,
        IFieldBuilder field,
        string parameterName
    )
    {
        IParameterBuilder parameterBuilder = constructorBuilder.AddParameter(builder =>
        {
            builder.Name = parameterName;
            builder.Type = field.Type;
        });

        constructorBuilder.AddBodyStatements($"{field.Name} = {parameterName};");
        return parameterBuilder;
    }


    public static IParameterBuilder AddParameter(this IMethodLikeBuilder methodBuilder, IType type, string name,
        string? initializer = null)
    {
        var parameterBuilder = new ParameterBuilder() {
            Type = type,
            Name = name,
            Initializer = initializer
        };
        methodBuilder.AddParameters(parameterBuilder);
        return parameterBuilder;
    }

    public static IParameterBuilder AddParameter(this IMethodLikeBuilder constructorBuilder, Action<IParameterBuilder> builder)
    {
        var parameterBuilder = new ParameterBuilder();
        constructorBuilder.AddParameters(parameterBuilder);
        builder(parameterBuilder);
        return parameterBuilder;
    }
}