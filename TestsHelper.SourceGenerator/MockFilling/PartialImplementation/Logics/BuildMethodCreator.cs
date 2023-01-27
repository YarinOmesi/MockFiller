using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsHelper.SourceGenerator.FluentSyntaxCreation;
using TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Models;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Logics;

public class BuildMethodCreator
{
    private static readonly SyntaxToken SemicolonToken = Token(SyntaxKind.SemicolonToken);

    public MethodDeclarationSyntax Create(
        WorkingClassInfo classInfo,
        IReadOnlyList<TypeMockResult> typeMockResults,
        List<ValueForParameter> valueForParameters)
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

        foreach (TypeMockResult result in typeMockResults)
        {
            // _parameterName = new Wrapper_Type(new Mock<>());
            StatementSyntax init = IdentifierName($"_{result.ParameterName}")
                .Assign(IdentifierName(result.Name).New(result.GeneratedMock.MockVariableType.New()))
                .ToStatement();
            
            body.Add(init);
        }

        // return <testedClassCreating>;
        body.Add(testedClassCreating.Return());
        method = method.WithBody(Block(body));

        return method;
    }
}