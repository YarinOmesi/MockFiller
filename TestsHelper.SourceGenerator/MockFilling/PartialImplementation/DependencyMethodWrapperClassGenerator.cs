using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using TestsHelper.SourceGenerator.CodeBuilding;
using TestsHelper.SourceGenerator.CodeBuilding.Abstractions;
using TestsHelper.SourceGenerator.CodeBuilding.Types;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation;

public class DependencyMethodWrapperClassGenerator
{
    public void CreateMethodWrapperClass(ITypeBuilder builder, IType dependencyTypeName, IMethodSymbol method)
    {
        builder.Name = $"Method_{method.Name}";
        builder.AddModifiers("public");


        IFieldBuilder expressionField = builder.AddField(builder =>
        {
            builder.Name = "_expression";
            IType moqCallbackType = method.ReturnType.SpecialType == SpecialType.System_Void
                ? "System".Type("Action").Generic(dependencyTypeName)
                : "System".Type("Func").Generic(dependencyTypeName, method.ReturnType.Type());

            builder.Type = CommonTypes.SystemLinqExpressions.Type("Expression").Generic(moqCallbackType);
            builder.Initializer = CreateMoqExpressionLambda(builder.Name, method);
        });
        expressionField.AddModifiers("private", "readonly");

        IFieldBuilder mockField = builder.AddField(Moq.Mock.Generic(dependencyTypeName), "_mock");
        mockField.AddModifiers("private", "readonly");

        IFieldBuilder converterField = builder.AddField(CommonTypes.ConverterType, "_converter");
        converterField.AddModifiers("private", "readonly");

        builder.AddConstructor((mockField, "mock"), (converterField, "converter"))
            .AddModifiers("public");


        IParameterBuilder[] parameters = method.Parameters
            .Select(parameter => (IParameterBuilder) new ParameterBuilder() {
                Type = CommonTypes.ValueType.Generic(parameter.Type.Type()),
                Name = $"{parameter.Name}",
                Initializer = $"default"
            })
            .ToArray();

        string patchedExpression = Cyber_CretePatchedExpression(method, expressionField.Name, converterField.Name);

        // Setup()
        builder.AddMethod(setupBuilder =>
        {
            setupBuilder.Name = "Setup";
            setupBuilder.ReturnType = method.ReturnType.SpecialType == SpecialType.System_Void
                ? Moq.ISetup.Generic(dependencyTypeName)
                : Moq.ISetup.Generic(dependencyTypeName, method.ReturnType.Type());

            string expressionVariableName = "expression";

            setupBuilder.AddParameters(parameters);
            setupBuilder.AddModifiers("public");
            setupBuilder.AddBodyStatements(
                $"var {expressionVariableName} = {patchedExpression};",
                $"return {mockField.Name}.Setup({expressionVariableName});"
            );
        });

        // Verify()
        builder.AddMethod(verifyBuilder =>
        {
            verifyBuilder.AddModifiers("public");
            verifyBuilder.ReturnType = VoidType.Instance;
            verifyBuilder.Name = "Verify";
            verifyBuilder.AddParameters(parameters);
            IParameterBuilder timesParameter = verifyBuilder.AddParameter(Moq.Times.Nullable(), "times", "null");
            string expressionVariableName = "expression";

            verifyBuilder.AddBodyStatements(
                $"var {expressionVariableName} = {patchedExpression};",
                $"{mockField.Name}.Verify({expressionVariableName}, {timesParameter.Name} ?? Times.AtLeastOnce());"
            );
        });
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