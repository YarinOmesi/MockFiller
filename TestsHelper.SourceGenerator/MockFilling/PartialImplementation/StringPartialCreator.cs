using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsHelper.SourceGenerator.CodeBuilding;
using TestsHelper.SourceGenerator.CodeBuilding.Abstractions;
using TestsHelper.SourceGenerator.CodeBuilding.Types;
using TestsHelper.SourceGenerator.MockFilling.Models;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation;

public class StringPartialCreator
{
    private readonly Dictionary<string, IDependencyBehavior> _dependencyBehaviors;
    private readonly ClassDeclarationSyntax _containingClassSyntax;
    private WrapperGenerationMode _generationMode;
    private readonly IMethodSymbol _selectedTestedClassConstructor;
    private readonly IType _testedClassType;
    private readonly DependencyWrapperGenerator _wrapperGenerator;

    public StringPartialCreator(
        Dictionary<string, IDependencyBehavior> dependencyBehaviors,
        ClassDeclarationSyntax containingClassSyntax,
        WrapperGenerationMode generationMode,
        IMethodSymbol selectedTestedClassConstructor, IType testedClassType)
    {
        _dependencyBehaviors = dependencyBehaviors;
        _containingClassSyntax = containingClassSyntax;
        _generationMode = generationMode;
        _selectedTestedClassConstructor = selectedTestedClassConstructor;
        _testedClassType = testedClassType;
        _wrapperGenerator = new DependencyWrapperGenerator(new DependencyMethodWrapperClassGenerator());
    }

    public void Build(IGeneratedResultBuilder resultBuilder)
    {
        string containingClassName = _containingClassSyntax.Identifier.Text;

        IFileBuilder partialClassFile = resultBuilder.CreateFileBuilder($"{containingClassName}.FilledMock.generated.cs");
        partialClassFile.Namespace = _containingClassSyntax.Parent is BaseNamespaceDeclarationSyntax parentNamespace
            ? parentNamespace.Name.ToString()
            : string.Empty;

        ITypeBuilder partialClassBuilder = partialClassFile.AddClass(name: containingClassName);
        partialClassBuilder.AddModifiers("public", "partial");

        Dictionary<string, string> parameterNameToFieldInitializer = new Dictionary<string, string>();


        IMethodBuilder buildMethodBuilder = partialClassBuilder.AddMethod(builder =>
        {
            builder.Name = "Build";
            builder.AddModifiers("private");
            builder.ReturnType = _testedClassType;
            builder.AddBodyStatements("var converter = MoqValueConverter.Instance;");
        });


        foreach (string parameterName in _dependencyBehaviors.Keys)
        {
            IDependencyBehavior behavior = _dependencyBehaviors[parameterName];

            if (behavior is MockDependencyBehavior mockDependencyBehavior)
            {
                IFileBuilder wrapperFile = resultBuilder.CreateFileBuilder($"Wrapper.{mockDependencyBehavior.WrapperClassName}.generated.cs");
                wrapperFile.Namespace = "TestsHelper.SourceGenerator.MockWrapping";

                ITypeBuilder dependencyWrapperType = wrapperFile.AddClass();
                _wrapperGenerator.GenerateCode(dependencyWrapperType, mockDependencyBehavior.Type);

                IFieldBuilder dependencyWrapperField = partialClassBuilder.AddField(dependencyWrapperType.Type(), $"_{parameterName}");
                dependencyWrapperField.AddModifiers("private");

                buildMethodBuilder.AddModifiers(
                    $"{dependencyWrapperField.Name} = new {dependencyWrapperField.Type.Name}(new Mock<{_testedClassType.Name}>(), converter);");
                parameterNameToFieldInitializer[parameterName] = $"{dependencyWrapperField.Name}.Mock.Object";

                wrapperFile.AddUsings(FindAllUsings(wrapperFile));
            }
            else if (behavior is PredefinedValueDependencyBehavior valueDependencyBehavior)
            {
                parameterNameToFieldInitializer[parameterName] = valueDependencyBehavior.VariableName;
            }
        }

        string arguments = _selectedTestedClassConstructor.Parameters
            .Select(parameter => parameterNameToFieldInitializer[parameter.Name])
            .JoinToString(", ");

        buildMethodBuilder.AddBodyStatements($"return new {_testedClassType.Name}({arguments});");

        partialClassFile.AddUsings(FindAllUsings(partialClassFile));
    }

    private string[] FindAllUsings(IFileBuilder builder)
    {
        IEnumerable<string> namespaces = TypesFinder.FindAllTypes(builder)
            .Select(type => type.Namespace)
            .Distinct();

        return namespaces.Select(type => $"using {type};").ToArray();
    }
    
    private class TypesFinder
    {
        private static IEnumerable<IType> FindAllTypesFromType(IType type)
        {
            if (type is not VoidType)
                yield return type;
            if (type is GenericType genericType) 
                foreach (IType t in genericType.TypedArguments.SelectMany(FindAllTypesFromType)) yield return t;
        }

        private static IEnumerable<IType> FindAllFromMember(IMemberBuilder memberBuilder)
        {
            if (memberBuilder is IPropertyBuilder propertyBuilder) 
                yield return propertyBuilder.Type;
            else if (memberBuilder is IFieldBuilder fieldBuilder) 
                yield return fieldBuilder.Type;
            else if (memberBuilder is ITypeBuilder typeBuilder) 
                foreach (IType type in FindAllTypes(typeBuilder)) yield return type;
            else if (memberBuilder is IMethodLikeBuilder methodLikeBuilder) 
                foreach (IParameterBuilder parameterBuilder in methodLikeBuilder.Parameters) yield return parameterBuilder.Type;
            if (memberBuilder is IMethodBuilder methodBuilder) 
                yield return methodBuilder.ReturnType;
        }

        public static IEnumerable<IType> FindAllTypes(ITypeBuilder typeBuilder)
        {
            return typeBuilder.Members
                .SelectMany(FindAllFromMember)
                .SelectMany(FindAllTypesFromType)
                .ToList();
        }

        public static IEnumerable<IType> FindAllTypes(IFileBuilder builder) => builder.Types.SelectMany(FindAllTypes).ToList();
    }
}

public enum WrapperGenerationMode
{
    OnlyMockWrap,
    MethodsWrap,
}

public interface IDependencyBehavior
{
}

public record PredefinedValueDependencyBehavior(string VariableName) : IDependencyBehavior;

public record MockDependencyBehavior(ITypeSymbol Type) : IDependencyBehavior
{
    public readonly string WrapperClassName = $"Wrapper_{Type.Name}";
}