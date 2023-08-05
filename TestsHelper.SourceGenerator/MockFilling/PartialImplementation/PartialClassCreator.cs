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
        string containingClassName,
        string containingNamespace,
        WrapperGenerationMode generationMode,
        IMethodSymbol selectedTestedClassConstructor,
        IType testedClassType
    )
    {
        IDependencyWrapperGenerator dependencyWrapperGenerator = DependencyWrapperGenerators[generationMode];

        List<FileBuilder> fileBuilders = new();

        var partialClassFile = FileBuilder.Create($"{containingClassName}.FilledMock.generated.cs");
        fileBuilders.Add(partialClassFile);

        partialClassFile.Namespace = containingNamespace;

        TypeBuilder partialClassBuilder = partialClassFile.AddClass(name: containingClassName)
            .Public().Partial();

        Dictionary<string, string> parameterNameToFieldInitializer = new Dictionary<string, string>();

        MethodBuilder buildMethodBuilder = MethodBuilder.Create(testedClassType, "Build").Add(partialClassBuilder)
            .Private();
        if (generationMode == WrapperGenerationMode.MethodsWrap)
        {
            buildMethodBuilder.AddBodyStatement($"var converter = {CommonTypes.MoqValueConverter}.Instance;");
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

                StringWithTypes[] parameters = new StringWithTypes[] {
                    StringWithTypes.Format($"new {Moq.Mock.Generic(mockDependencyBehavior.Type)}()"),
                    StringWithTypes.Format($"converter").TakeIf(generationMode == WrapperGenerationMode.MethodsWrap)
                };
                
                buildMethodBuilder.AddBodyStatement($"{dependencyWrapperField} = new {dependencyWrapperType}({parameters:,});");

                // TODO: remove coupling from moq
                parameterNameToFieldInitializer[parameterName] = $"{dependencyWrapperField.Name}.Mock.Object";

            }
            else if (initializationBehavior is PredefinedValueDependencyInitialization valueDependencyBehavior)
            {
                parameterNameToFieldInitializer[parameterName] = valueDependencyBehavior.VariableName;
            }
        }

        string[] arguments = selectedTestedClassConstructor.Parameters
            .Select(parameter => parameterNameToFieldInitializer[parameter.Name])
            .ToArray();

        buildMethodBuilder.AddBodyStatement($"return new {testedClassType}({arguments:,});");

        return fileBuilders;
    }
}