using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsHelper.SourceGenerator.CodeBuilding;
using TestsHelper.SourceGenerator.CodeBuilding.Abstractions;
using TestsHelper.SourceGenerator.CodeBuilding.Types;
using TestsHelper.SourceGenerator.MockFilling.PartialImplementation.DependencyMethodWrapperGenerator;
using TestsHelper.SourceGenerator.MockFilling.PartialImplementation.DependencyWrapperGenerators;
using TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Types;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation;

public static class StringPartialCreator
{
    private static readonly Dictionary<WrapperGenerationMode, IDependencyWrapperGenerator> DependencyWrapperGenerators = new() {
        [WrapperGenerationMode.MethodsWrap] = new DependencyWrapperGenerator(new DependencyMethodWrapperClassGenerator()),
        [WrapperGenerationMode.OnlyMockWrap] = new NoWrappingDependencyWrapperGenerator(new NoWrappingDependencyMethodGenerator())
    };

    public static List<FileBuilder> Create(
        Dictionary<string, IDependencyBehavior> dependencyBehaviors,
        ClassDeclarationSyntax containingClassSyntax,
        WrapperGenerationMode generationMode,
        IMethodSymbol selectedTestedClassConstructor,
        IType testedClassType
    )
    {
        IDependencyWrapperGenerator dependencyWrapperGenerator = DependencyWrapperGenerators[generationMode];

        List<FileBuilder> fileBuilders = new();

        string containingClassName = containingClassSyntax.Identifier.Text;

        var partialClassFile = FileBuilder.Create($"{containingClassName}.FilledMock.generated.cs");
        fileBuilders.Add(partialClassFile);

        partialClassFile.Namespace = containingClassSyntax.Parent is BaseNamespaceDeclarationSyntax parentNamespace
            ? parentNamespace.Name.ToString()
            : string.Empty;

        ITypeBuilder partialClassBuilder = partialClassFile.AddClass(name: containingClassName)
            .Public().Partial();

        Dictionary<string, string> parameterNameToFieldInitializer = new Dictionary<string, string>();


        MethodBuilder buildMethodBuilder = MethodBuilder.Create(testedClassType, "Build").Add(partialClassBuilder)
            .Private();
        if (generationMode == WrapperGenerationMode.MethodsWrap)
        {
            buildMethodBuilder.AddBodyStatements($"var converter = {CommonTypes.MoqValueConverter.Qualify().MakeString()}.Instance;");
        }

        foreach (string parameterName in dependencyBehaviors.Keys)
        {
            IDependencyBehavior behavior = dependencyBehaviors[parameterName];

            if (behavior is MockDependencyBehavior mockDependencyBehavior)
            {
                var wrapperFile = FileBuilder.Create($"Wrapper.{mockDependencyBehavior.WrapperClassName}.generated.cs");
                fileBuilders.Add(wrapperFile);

                wrapperFile.Namespace = "TestsHelper.SourceGenerator.MockWrapping";

                ITypeBuilder dependencyWrapperType = wrapperFile.AddClass();
                dependencyWrapperGenerator.GenerateCode(dependencyWrapperType, mockDependencyBehavior.Type);

                FieldBuilder dependencyWrapperField = FieldBuilder.Create(dependencyWrapperType.Type(), $"_{parameterName}")
                    .Add(partialClassBuilder)
                    .Private();

                buildMethodBuilder.AddBodyStatements(
                    generationMode == WrapperGenerationMode.MethodsWrap
                        ? $"{dependencyWrapperField.Name} = new {dependencyWrapperField.Type.Name}(new {Moq.Mock.Qualify().Generic(mockDependencyBehavior.Type.Type()).MakeString()}(), converter);"
                        : $"{dependencyWrapperField.Name} = new {dependencyWrapperField.Type.Name}(new {Moq.Mock.Qualify().Generic(mockDependencyBehavior.Type.Type()).MakeString()}());"
                );
                parameterNameToFieldInitializer[parameterName] = $"{dependencyWrapperField.Name}.Mock.Object";

                wrapperFile.AddUsings(FindAllUsingsNamespaces(wrapperFile));
            }
            else if (behavior is PredefinedValueDependencyBehavior valueDependencyBehavior)
            {
                parameterNameToFieldInitializer[parameterName] = valueDependencyBehavior.VariableName;
            }
        }

        string arguments = selectedTestedClassConstructor.Parameters
            .Select(parameter => parameterNameToFieldInitializer[parameter.Name])
            .JoinToString(", ");

        buildMethodBuilder.AddBodyStatements($"return new {testedClassType.Name}({arguments});");

        partialClassFile.AddUsings(FindAllUsingsNamespaces(partialClassFile));

        return fileBuilders;
    }

    private static string[] FindAllUsingsNamespaces(IFileBuilder builder)
    {
        return TypesFinder.FindAllTypes(builder).Select(type => type.Namespace).Distinct().ToArray();
    }

    private static class TypesFinder
    {
        private static IEnumerable<IType> FindAllTypesFromType(IType type)
        {
            if (type is not VoidType)
                yield return type;
            if (type is GenericType genericType)
                foreach (IType t in genericType.TypedArguments.SelectMany(FindAllTypesFromType))
                    yield return t;
        }

        private static IEnumerable<IType> FindAllFromMember(IMemberBuilder memberBuilder)
        {
            if (memberBuilder is PropertyBuilder propertyBuilder)
                yield return propertyBuilder.Type;
            else if (memberBuilder is FieldBuilder fieldBuilder)
                yield return fieldBuilder.Type;
            else if (memberBuilder is ITypeBuilder typeBuilder)
                foreach (IType type in FindAllTypes(typeBuilder))
                    yield return type;
            else if (memberBuilder is MethodLikeBuilder methodLikeBuilder)
                foreach (IParameterBuilder parameterBuilder in methodLikeBuilder.Parameters)
                    yield return parameterBuilder.Type;
            if (memberBuilder is MethodBuilder methodBuilder)
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