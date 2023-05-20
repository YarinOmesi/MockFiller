using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsHelper.SourceGenerator.CodeBuilding;
using TestsHelper.SourceGenerator.CodeBuilding.Types;
using TestsHelper.SourceGenerator.MockFilling.PartialImplementation.DependencyMethodWrapperGenerator;
using TestsHelper.SourceGenerator.MockFilling.PartialImplementation.DependencyWrapperGenerators;
using TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Types;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation;

public static class StringPartialCreator
{
    private static readonly Dictionary<WrapperGenerationMode, BaseDependencyWrapperGenerator> DependencyWrapperGenerators = new() {
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
        BaseDependencyWrapperGenerator dependencyWrapperGenerator = DependencyWrapperGenerators[generationMode];

        List<FileBuilder> fileBuilders = new();

        string containingClassName = containingClassSyntax.Identifier.Text;

        var partialClassFile = FileBuilder.Create($"{containingClassName}.FilledMock.generated.cs");
        fileBuilders.Add(partialClassFile);

        partialClassFile.Namespace = containingClassSyntax.Parent is BaseNamespaceDeclarationSyntax parentNamespace
            ? parentNamespace.Name.ToString()
            : string.Empty;

        TypeBuilder partialClassBuilder = partialClassFile.AddClass(name: containingClassName)
            .Public().Partial();

        partialClassFile.AddUsingFor(Moq.Mock);
        
        Dictionary<string, string> parameterNameToFieldInitializer = new Dictionary<string, string>();


        MethodBuilder buildMethodBuilder = MethodBuilder.Create(testedClassType, "Build").Add(partialClassBuilder)
            .Private();
        if (generationMode == WrapperGenerationMode.MethodsWrap)
        {
            partialClassFile.AddUsingFor(CommonTypes.MoqValueConverter);
            buildMethodBuilder.AddBodyStatements($"var converter = {CommonTypes.MoqValueConverter.MakeString()}.Instance;");
        }

        foreach (string parameterName in dependencyBehaviors.Keys)
        {
            IDependencyBehavior behavior = dependencyBehaviors[parameterName];

            if (behavior is MockDependencyBehavior mockDependencyBehavior)
            {
                var wrapperFile = FileBuilder.Create($"Wrapper.{mockDependencyBehavior.Type.Name}.generated.cs");
                fileBuilders.Add(wrapperFile);

                wrapperFile.Namespace = "TestsHelper.SourceGenerator.MockWrapping";

                TypeBuilder dependencyWrapperType = wrapperFile.AddClass();
                dependencyWrapperGenerator.GenerateCode(dependencyWrapperType, mockDependencyBehavior.Type);

                FieldBuilder dependencyWrapperField = FieldBuilder.Create(dependencyWrapperType.Type(), $"_{parameterName}")
                    .Add(partialClassBuilder)
                    .Private();

                List<string> parameters = new() {Moq.Mock.Generic(mockDependencyBehavior.Type.Type()).New()};
                if(generationMode == WrapperGenerationMode.MethodsWrap) parameters.Add("converter");
                
                buildMethodBuilder.AddBodyStatements(dependencyWrapperField.Assign(dependencyWrapperType.Type().New(parameters.ToArray())));

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

        buildMethodBuilder.AddBodyStatements($"return {testedClassType.New(arguments)};");

        partialClassFile.AddUsings(FindAllUsingsNamespaces(partialClassFile));

        return fileBuilders;
    }

    private static string[] FindAllUsingsNamespaces(FileBuilder builder)
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

        private static IEnumerable<IType> FindAllFromMember(MemberBuilder memberBuilder)
        {
            if (memberBuilder is PropertyBuilder propertyBuilder)
                yield return propertyBuilder.Type;
            else if (memberBuilder is FieldBuilder fieldBuilder)
                yield return fieldBuilder.Type;
            else if (memberBuilder is TypeBuilder typeBuilder)
                foreach (IType type in FindAllTypes(typeBuilder))
                    yield return type;
            else if (memberBuilder is MethodLikeBuilder methodLikeBuilder)
                foreach (ParameterBuilder parameterBuilder in methodLikeBuilder.Parameters)
                    yield return parameterBuilder.Type;
            if (memberBuilder is MethodBuilder methodBuilder)
                yield return methodBuilder.ReturnType;
        }

        public static IEnumerable<IType> FindAllTypes(TypeBuilder typeBuilder)
        {
            return typeBuilder.Members
                .SelectMany(FindAllFromMember)
                .SelectMany(FindAllTypesFromType)
                .ToList();
        }

        public static IEnumerable<IType> FindAllTypes(FileBuilder builder) => builder.Types.SelectMany(FindAllTypes).ToList();
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

public record MockDependencyBehavior(ITypeSymbol Type) : IDependencyBehavior;