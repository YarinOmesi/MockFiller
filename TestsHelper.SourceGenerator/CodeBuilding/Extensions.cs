using System;
using TestsHelper.SourceGenerator.CodeBuilding.Abstractions;
using TestsHelper.SourceGenerator.CodeBuilding.Types;

namespace TestsHelper.SourceGenerator.CodeBuilding;

public static class Extensions
{
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

    public static IFieldBuilder AddField(this ITypeBuilder typeBuilder, IType type, string name, string? initializer = null)
    {
        var fieldBuilder = new FieldBuilder {
            Type = type,
            Name = name,
            Initializer = initializer
        };
        typeBuilder.AddMembers(fieldBuilder);
        return fieldBuilder;
    }

    public static IPropertyBuilder AddProperty(this ITypeBuilder builder, IType type, string name)
    {
        var propertyBuilder = new PropertyBuilder {
            Type = type,
            Name = name
        };
        builder.AddMembers(propertyBuilder);

        return propertyBuilder;
    }

    public static IMethodBuilder AddMethod(this ITypeBuilder type, Action<IMethodBuilder> builder)
    {
        var methodBuilder = new MethodBuilder();
        type.AddMembers(methodBuilder);
        builder(methodBuilder);
        return methodBuilder;
    }

    public static IConstructorBuilder AddConstructor(this ITypeBuilder type, params (IFieldBuilder, string)[] fieldsToInitialize)
    {
        var constructorBuilder = new ConstructorBuilder(type);

        foreach ((IFieldBuilder fieldBuilder, string parameterName) in fieldsToInitialize)
        {
            constructorBuilder.InitializeFieldWithParameter(fieldBuilder, parameterName);
        }

        return constructorBuilder;
    }

    public static IParameterBuilder InitializeFieldWithParameter(this IConstructorBuilder builder, IFieldBuilder field, string parameterName)
    {
        IParameterBuilder parameterBuilder = new ParameterBuilder() {Name = parameterName, Type = field.Type};
        builder.AddParameters(parameterBuilder);
        builder.AddBodyStatements($"{field.Name} = {parameterName};");
        return parameterBuilder;
    }

    public static IParameterBuilder AddParameter(this IMethodLikeBuilder builder, IType type, string name, string? initializer = null)
    {
        var parameterBuilder = new ParameterBuilder() {
            Type = type,
            Name = name,
            Initializer = initializer
        };
        builder.AddParameters(parameterBuilder);
        return parameterBuilder;
    }
}