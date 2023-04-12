using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsHelper.SourceGenerator.FluentSyntaxCreation;
using TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Models;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Logics;

public class BuildMethodCreator
{
    public MethodDeclarationSyntax Create(
        WorkingClassInfo classInfo,
        IReadOnlyList<TypeMockResult> typeMockResults,
        List<ValueForParameter> valueForParameters,
        bool generateWrappers)
    {
        IdentifierNameSyntax objectToBuild = IdentifierName(classInfo.SelectedConstructor.ContainingType.Name);
        // private <TestedClass> Build()
        MethodDeclarationSyntax method = MethodDeclaration(objectToBuild, "Build")
            .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)));


        Dictionary<string, ExpressionSyntax> argumentsByName = new();

        foreach (TypeMockResult result in typeMockResults)
        {
            argumentsByName[result.ParameterName] = $"_{result.ParameterName}"
                .AccessMember(result.MockPropertyName)
                .AccessMember("Object");
        }

        foreach (ValueForParameter valueForParameter in valueForParameters)
        {
            argumentsByName[valueForParameter.ParameterName] = IdentifierName(valueForParameter.Name);
        }

        List<ExpressionSyntax> arguments = classInfo.SelectedConstructor.Parameters
            .Select(parameter => argumentsByName[parameter.Name])
            .ToList();

        // new TestedClass(arguments)
        ObjectCreationExpressionSyntax testedClassCreating = objectToBuild.New(arguments: arguments.ToArray());

        List<StatementSyntax> body = new();

        if (generateWrappers)
        {
            var converterVariableName = "converter";
            
            // var converter = MoqValueConverter.Instance;
            body.Add(LocalDeclarationStatement(
                converterVariableName.DeclareVariable("MoqValueConverter".AccessMember("Instance"))
            ));

            // _parameterName = new Wrapper_Type(new Mock<>, converter);
            // ....
            body.AddRange(typeMockResults.Select(result => 
                CreateMockWrapperInitialization(result, IdentifierName(converterVariableName)))
            );
        }
        else
        {
            // _parameterName = new Wrapper_Type(new Mock<>);
            // ....
            body.AddRange(typeMockResults.Select(result => CreateMockWrapperInitialization(result)));
        }

        // return <testedClassCreating>;
        body.Add(testedClassCreating.Return());
        method = method.WithBody(Block(body));

        return method;
    }

    private static ExpressionStatementSyntax CreateMockWrapperInitialization(TypeMockResult result, params ExpressionSyntax[] moreWrapperArgs)
    {
        List<ExpressionSyntax> wrapperArguments = new() {
            // new Mock<>()
            result.GeneratedMock.MockVariableType.New()
        };
        wrapperArguments.AddRange(moreWrapperArgs);

        return IdentifierName($"_{result.ParameterName}")
            .Assign(IdentifierName(result.Name).New(wrapperArguments.ToArray()))
            .ToStatement();
    }
}