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

        IType moqCallbackType = method.ReturnType.SpecialType == SpecialType.System_Void
            ? "System".Type("Action").Generic(dependencyTypeName)
            : "System".Type("Func").Generic(dependencyTypeName, method.ReturnType.Type());

        FieldBuilder expressionField = FieldBuilder.Create(
            CommonTypes.LinqExpression.Generic(moqCallbackType),
            "_expression",
            CreateMoqExpressionLambda(builder.Name, method)
        ).Add(builder);

        expressionField.AddModifiers("private", "readonly");

        FieldBuilder mockField = FieldBuilder.Create(Moq.Mock.Generic(dependencyTypeName), "_mock").Add(builder);
        mockField.AddModifiers("private", "readonly");

        FieldBuilder converterField = FieldBuilder.Create(CommonTypes.ConverterType, "_converter").Add(builder);
        converterField.AddModifiers("private", "readonly");

        ConstructorBuilder.CreateAndAdd(builder)
            .InitializeFieldWithParameters((mockField, "mock"), (converterField, "converter"))
            .AddModifiers("public");

        IParameterBuilder[] parameters = method.Parameters
            .Select(parameter => (IParameterBuilder) ParameterBuilder.Create(
                type: CommonTypes.ValueType.Generic(parameter.Type.Type()),
                name: $"{parameter.Name}",
                initializer: $"default"
            ))
            .ToArray();

        string patchedExpression = Cyber_CretePatchedExpression(method, expressionField.Name, converterField.Name);

        // Setup()
        var setupReturnType = method.ReturnType.SpecialType == SpecialType.System_Void
            ? Moq.ISetup.Generic(dependencyTypeName)
            : Moq.ISetup.Generic(dependencyTypeName, method.ReturnType.Type());

        var setupBuilder = MethodBuilder.Create(setupReturnType, "Setup", parameters).Add(builder);
        setupBuilder.AddModifiers("public");
        setupBuilder.AddBodyStatements(
            $"var expression = {patchedExpression};",
            $"return {mockField.Name}.Setup(expression);"
        );

        // Verify()
        var verifyBuilder = MethodBuilder.Create(VoidType.Instance, "Verify", parameters).Add(builder);
        verifyBuilder.AddModifiers("public");
        IParameterBuilder timesParameter = ParameterBuilder.Create(Moq.Times.Nullable(), "times", "null")
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