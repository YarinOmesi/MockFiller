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

public static class PartialClassCreator
{
    private static readonly Dictionary<WrapperGenerationMode, IDependencyWrapperGenerator> DependencyWrapperGenerators = new() {
        [WrapperGenerationMode.MethodsWrap] = new DependencyWrapperGenerator(new DependencyMethodWrapperClassGenerator()),
        [WrapperGenerationMode.OnlyMockWrap] = new NoWrappingDependencyWrapperGenerator()
    };

    public static List<FileBuilder> Create(
        Dictionary<string, IDependencyInitializationBehavior> dependencyBehaviors,
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
            IDependencyInitializationBehavior initializationBehavior = dependencyBehaviors[parameterName];

            if (initializationBehavior is MockDependencyInitialization mockDependencyBehavior)
            {
                var wrapperFile = FileBuilder.Create($"Wrapper.{mockDependencyBehavior.Type.Name}.generated.cs");
                fileBuilders.Add(wrapperFile);

                wrapperFile.Namespace = "TestsHelper.SourceGenerator.MockWrapping";

                TypeBuilder dependencyWrapperType = wrapperFile.AddClass();
                dependencyWrapperGenerator.GenerateCode(dependencyWrapperType, mockDependencyBehavior.Type);

                FieldBuilder dependencyWrapperField = FieldBuilder.Create(dependencyWrapperType.Type(), $"_{parameterName}")
                    .Add(partialClassBuilder)
                    .Private();

                List<string> parameters = new() {Moq.Mock.Generic(mockDependencyBehavior.Type).New()};
                if(generationMode == WrapperGenerationMode.MethodsWrap) parameters.Add("converter");
                
                buildMethodBuilder.AddBodyStatements(dependencyWrapperField.Assign(dependencyWrapperType.Type().New(parameters.ToArray())));

                // TODO: remove coupling from moq
                parameterNameToFieldInitializer[parameterName] = $"{dependencyWrapperField.Name}.Mock.Object";

                wrapperFile.AddUsings(FindAllUsingsNamespaces(wrapperFile));
            }
            else if (initializationBehavior is PredefinedValueDependencyInitialization valueDependencyBehavior)
            {
                parameterNameToFieldInitializer[parameterName] = valueDependencyBehavior.VariableName;
            }
        }

        string[] arguments = selectedTestedClassConstructor.Parameters
            .Select(parameter => parameterNameToFieldInitializer[parameter.Name])
            .ToArray();

        buildMethodBuilder.AddBodyStatements(testedClassType.New(arguments).Return());

        partialClassFile.AddUsings(FindAllUsingsNamespaces(partialClassFile));

        return fileBuilders;
    }

    private static string[] FindAllUsingsNamespaces(FileBuilder builder)
    {
        return TypesFinder.FindAllTypes(builder).Select(type => type.Namespace).Distinct().ToArray();
    }

    private static class TypesFinder
    {
        private static IReadOnlyList<IType> FindAllTypesFromType(IType type)
        {
            List<IType> types = new List<IType>();
            if (type is not VoidType) types.Add(type);
            if (type is RegularType genericType) types.AddRange(genericType.TypedArguments.SelectMany(FindAllTypesFromType));
            return types;
        }

        private static IEnumerable<IType> FindAllFromMember(MemberBuilder memberBuilder)
        {
            List<IType> types = new List<IType>();
            if (memberBuilder is PropertyBuilder propertyBuilder) types.Add(propertyBuilder.Type);
            else if (memberBuilder is FieldBuilder fieldBuilder) types.Add(fieldBuilder.Type);
            else if (memberBuilder is TypeBuilder typeBuilder) types.AddRange(FindAllTypes(typeBuilder));
            else if (memberBuilder is MethodLikeBuilder methodLikeBuilder) 
                types.AddRange(methodLikeBuilder.Parameters.Select(builder => builder.Type));
            if (memberBuilder is MethodBuilder methodBuilder) types.Add(methodBuilder.ReturnType);
            return types;
        }

        public static IEnumerable<IType> FindAllTypes(TypeBuilder typeBuilder)
        {
            return typeBuilder.Members.SelectMany(FindAllFromMember).SelectMany(FindAllTypesFromType).ToList();
        }

        public static IEnumerable<IType> FindAllTypes(FileBuilder builder) => builder.Types.SelectMany(FindAllTypes).ToList();
    }
}