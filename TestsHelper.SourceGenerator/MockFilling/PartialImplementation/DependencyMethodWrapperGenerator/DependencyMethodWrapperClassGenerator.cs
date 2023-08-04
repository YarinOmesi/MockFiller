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


        IEnumerable<IType> methodParameter = method.Parameters.Select(symbol => symbol.Type.Type().TryRegisterAlias(builder.ParentFileBuilder));
        
        FieldBuilder expressionField = FieldBuilder.Create(
            CommonTypes.LinqExpression.Generic(moqCallbackType),
            "_expression",
            CreateMoqExpressionLambda("p", method.Name ,methodParameter)
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

        string patchedExpression = Cyber_CretePatchedExpression(method, expressionField.Name, converterField.Name);

        // Setup()
        var setupReturnType = Moq.ISetup with {TypedArguments = moqCallbackType.TypedArguments};

        var setupBuilder = MethodBuilder.Create(setupReturnType, "Setup", parameters).Add(builder)
            .Public();
        setupBuilder.AddBodyStatements(
            $"var expression = {patchedExpression};",
            $"return {mockField.Name}.Setup(expression);"
        );

        // Verify()
        var verifyBuilder = MethodBuilder.Create(VoidType.Instance, "Verify", parameters).Add(builder)
            .Public();
        ParameterBuilder timesParameter = ParameterBuilder.Create(Moq.Times.Nullable(), "times", "null");
        verifyBuilder.AddParameters(timesParameter);

        string timesTypeName = Moq.Times.TryRegisterAlias(builder.ParentFileBuilder).Name;
        verifyBuilder.AddBodyStatements(
            $"var expression = {patchedExpression};",
            $"{mockField.Name}.Verify(expression, {timesParameter.Name} ?? {timesTypeName}.AtLeastOnce());"
        );
    }

    private static string CreateMoqExpressionLambda(string parameterName,string methodName, IEnumerable<IType> method)
    {
        IEnumerable<string> allParameterTypesFilled = method.Select(parameter => Cyber_Fill(parameter.Name));

        return $"{parameterName} => {parameterName}.{methodName}({allParameterTypesFilled.JoinToString(", ")})";
    }

    private static string Cyber_CretePatchedExpression(IMethodSymbol method, string variableName, string converterFieldName)
    {
        IEnumerable<string> converterParameters = method.Parameters.Select(parameter => $"{converterFieldName}.Convert({parameter.Name})");
        IEnumerable<string> parameters = converterParameters.Prepend(variableName);

        return $"Cyber.UpdateExpressionWithParameters({parameters.JoinToString(", ")})";
    }

    private static string Cyber_Fill(string type) => $"Cyber.Fill<{type}>()";
}