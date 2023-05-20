using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using TestsHelper.SourceGenerator.CodeBuilding;
using TestsHelper.SourceGenerator.CodeBuilding.Types;
using TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Types;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation.DependencyMethodWrapperGenerator;

public class DependencyMethodWrapperClassGenerator : IDependencyMethodClassGenerator
{
    public void CreateMethodWrapperClass(TypeBuilder builder, IType dependencyTypeName, IMethodSymbol method)
    {
        builder.Name = $"Method_{method.Name}";
        builder.Public();

        IType moqCallbackType = method.ReturnType.SpecialType == SpecialType.System_Void
            ? CommonTypes.SystemAction.Generic(dependencyTypeName)
            : CommonTypes.SystemFunc.Generic(dependencyTypeName, method.ReturnType.Type());

        FieldBuilder expressionField = FieldBuilder.Create(
            CommonTypes.LinqExpression.Generic(moqCallbackType),
            "_expression",
            CreateMoqExpressionLambda("p", method)
        ).Add(builder).Private().Readonly();

        FieldBuilder mockField = FieldBuilder.Create(Moq.Mock.Generic(dependencyTypeName), "_mock").Add(builder);
        mockField.Private().Readonly();

        FieldBuilder converterField = FieldBuilder.Create(CommonTypes.ConverterType, "_converter").Add(builder);
        converterField.Private().Readonly();

        ConstructorBuilder.CreateAndAdd(builder)
            .InitializeFieldWithParameters((mockField, "mock"), (converterField, "converter"))
            .Public();

        ParameterBuilder[] parameters = method.Parameters
            .Select(parameter => (ParameterBuilder) ParameterBuilder.Create(
                type: CommonTypes.ValueType.Generic(parameter.Type.Type()),
                name: parameter.Name,
                initializer: "default"
            ))
            .ToArray();

        string patchedExpression = Cyber_CretePatchedExpression(method, expressionField.Name, converterField.Name);

        // Setup()
        var setupReturnType = method.ReturnType.SpecialType == SpecialType.System_Void
            ? Moq.ISetup.Generic(dependencyTypeName)
            : Moq.ISetup.Generic(dependencyTypeName, method.ReturnType.Type());

        var setupBuilder = MethodBuilder.Create(setupReturnType, "Setup", parameters).Add(builder)
            .Public();
        setupBuilder.AddBodyStatements(
            $"var expression = {patchedExpression};",
            $"return {mockField.Name}.Setup(expression);"
        );

        // Verify()
        var verifyBuilder = MethodBuilder.Create(VoidType.Instance, "Verify", parameters).Add(builder)
            .Public();
        ParameterBuilder timesParameter = ParameterBuilder.Create(Moq.Times.Nullable(), "times", "null")
            .Add(verifyBuilder);

        verifyBuilder.AddBodyStatements(
            $"var expression = {patchedExpression};",
            $"{mockField.Name}.Verify(expression, {timesParameter.Name} ?? Times.AtLeastOnce());"
        );
    }

    private static string CreateMoqExpressionLambda(string parameterName, IMethodSymbol method)
    {
        List<string> allParameterTypesFilled = method.Parameters
            .Select(parameter => Cyber_Fill(parameter.Type.Name))
            .ToList();

        return $"{parameterName} => {parameterName}.{method.Name}({allParameterTypesFilled.JoinToString(", ")})";
    }

    private static string Cyber_CretePatchedExpression(IMethodSymbol method, string variableName, string converterFieldName)
    {
        List<string> parameters = new();
        parameters.Add(variableName);
        parameters.AddRange(method.Parameters.Select(parameter => $"{converterFieldName}.Convert({parameter.Name})"));

        return $"Cyber.UpdateExpressionWithParameters({parameters.JoinToString(", ")})";
    }

    private static string Cyber_Fill(string type) => $"Cyber.Fill<{type}>()";
}