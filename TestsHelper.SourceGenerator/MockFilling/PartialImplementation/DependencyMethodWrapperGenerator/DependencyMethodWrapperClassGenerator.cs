using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using TestsHelper.SourceGenerator.CodeBuilding;
using TestsHelper.SourceGenerator.CodeBuilding.Types;
using TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Types;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation.DependencyMethodWrapperGenerator;

public class DependencyMethodWrapperClassGenerator : IDependencyMethodWrapperClassGenerator
{
    public void Generate(TypeBuilder builder, IType dependencyTypeName, IMethodSymbol method)
    {
        builder.Name = $"Method_{method.Name}";
        builder.Public();

        RegularType moqCallbackType = method.ReturnType.SpecialType == SpecialType.System_Void
            ? CommonTypes.SystemAction.Generic(dependencyTypeName)
            : CommonTypes.SystemFunc.Generic(dependencyTypeName, method.ReturnType.Type());

        FieldBuilder expressionField = FieldBuilder.Create(
            CommonTypes.LinqExpression.Generic(moqCallbackType),
            "_expression",
            CreateMoqExpressionLambda("p", method)
        ).Add(builder).Private().Readonly();

        FieldBuilder mockField = FieldBuilder.Create(Moq.Mock.Generic(dependencyTypeName), "_mock").Add(builder);
        mockField.Private().Readonly();

        FieldBuilder converterField = FieldBuilder.Create(CommonTypes.IValueConverter, "_converter").Add(builder);
        converterField.Private().Readonly();

        ConstructorBuilder.CreateAndAdd(builder)
            .InitializeFieldWithParameters((mockField, "mock"), (converterField, "converter"))
            .Public();

        ParameterBuilder[] parameters = method.Parameters
            .Select(parameter => ParameterBuilder.Create(
                type: CommonTypes.Value.Generic(parameter.Type),
                name: parameter.Name,
                initializer: "default"
            ))
            .ToArray();

        StringWithTypes patchedExpression = Cyber_CretePatchedExpression(method, expressionField.Name, converterField.Name);

        // Setup()
        var setupReturnType = Moq.ISetup with {TypedArguments = moqCallbackType.TypedArguments};

        var setupBuilder = MethodBuilder.Create(setupReturnType, "Setup", parameters).Add(builder)
            .Public();
        setupBuilder.AddBodyStatement($"var expression = {patchedExpression};");
        setupBuilder.AddBodyStatement($"return {mockField}.Setup(expression);");

        // Verify()
        var verifyBuilder = MethodBuilder.Create(VoidType.Instance, "Verify", parameters).Add(builder)
            .Public();
        ParameterBuilder timesParameter = ParameterBuilder.Create(Moq.Times.Nullable(), "times", "null");
        verifyBuilder.AddParameters(timesParameter);

        verifyBuilder.AddBodyStatement($"var expression = {patchedExpression};");
        verifyBuilder.AddBodyStatement($"{mockField}.Verify(expression, {timesParameter} ?? {Moq.Times}.AtLeastOnce());");
    }

    private static StringWithTypes CreateMoqExpressionLambda(string parameterName, IMethodSymbol method)
    {
        IEnumerable<IType> methodParameters = method.Parameters.Select(symbol => symbol.Type.Type());

        return StringWithTypes.Format($"{parameterName} => {parameterName}.{method.Name}({methodParameters.Select(Cyber_Fill):,})");
    }

    private static StringWithTypes Cyber_CretePatchedExpression(IMethodSymbol method, string variableName, string converterFieldName)
    {
        IEnumerable<string> converterParameters = method.Parameters.Select(parameter => $"{converterFieldName}.Convert({parameter.Name})");
        IEnumerable<string> parameters = converterParameters.Prepend(variableName);

        return StringWithTypes.Format($"{CommonTypes.Cyber}.UpdateExpressionWithParameters({parameters:,})");
    }

    private static StringWithTypes Cyber_Fill(IType type) => StringWithTypes.Format($"{CommonTypes.Cyber}.Fill<{type}>()");
}