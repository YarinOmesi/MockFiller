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
        GeneratedMock[] generatedMocks,
        List<ValueForParameter> valueForParameters)
    {
        IdentifierNameSyntax objectToBuild = IdentifierName(classInfo.SelectedConstructor.ContainingType.Name);
        // private <TestedClass> Build()
        MethodDeclarationSyntax method = MethodDeclaration(objectToBuild, "Build")
            .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)));


        Dictionary<string, ExpressionSyntax> argumentsByName = new();

        foreach (GeneratedMock generatedMock in generatedMocks)
        {
            argumentsByName[generatedMock.ParameterName] = generatedMock.MockVariableName.AccessMember("Object");
        }

        foreach (ValueForParameter valueForParameter in valueForParameters)
        {
            argumentsByName[valueForParameter.ParameterName] = IdentifierName(valueForParameter.Name);
        }

        List<ArgumentSyntax> arguments = classInfo.SelectedConstructor.Parameters
            .Select(parameter => Argument(argumentsByName[parameter.Name]))
            .ToList();

        // new TestedClass(arguments)
        ObjectCreationExpressionSyntax testedClassCreating = ObjectCreationExpression(objectToBuild)
            .WithArgumentList(ArgumentList(SeparatedList(arguments)));

        List<StatementSyntax> body = new();

        foreach (GeneratedMock generatedMock in generatedMocks)
        {
            // _mock = new Mock<>();
            body.Add(IdentifierName(generatedMock.MockVariableName)
                .Assign(ObjectCreationExpression(generatedMock.MockVariableType).WithArgumentList(ArgumentList()))
                .ToStatement()
            );
        }

        // return <testedClassCreating>;
        body.Add(ReturnStatement(testedClassCreating).WithSemicolonToken(SemicolonToken));
        method = method.WithBody(Block(body));

        return method;
    }
}