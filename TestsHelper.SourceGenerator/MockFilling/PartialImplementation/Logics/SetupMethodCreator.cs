using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsHelper.SourceGenerator.FluentSyntaxCreation;
using TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Models;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Logics;

public class SetupMethodCreator
{
    private static readonly SyntaxToken SemicolonToken = Token(SyntaxKind.SemicolonToken);


    private static readonly string[] CyberUsings = new[] {
        "TestsHelper.SourceGenerator.MockWrapping",
        "Moq",
        "Moq.Language.Flow",
        "System.Linq.Expressions",
        "System",
        "System.Collections.Generic",
        "System.Linq"
    };

    public SetupMethodResult Create(IEnumerable<GeneratedMock> generatedMocks)
    {
        return new SetupMethodResult(CyberUsings, generatedMocks.SelectMany(CreateForMock).ToList());
    }

    private static List<MethodDeclarationSyntax> CreateForMock(GeneratedMock generatedMock)
    {
        List<MethodDeclarationSyntax> methods = new();

        ITypeSymbol mockedClassType = generatedMock.Mock.Type;
        List<IMethodSymbol> publicMethods = mockedClassType.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(method => method.DeclaredAccessibility == Accessibility.Public)
            .ToList();


        foreach (IMethodSymbol method in publicMethods)
        {
            GenericNameSyntax callback;
            GenericNameSyntax returnType;

            if (method.ReturnType.SpecialType == SpecialType.System_Void)
            {
                callback = "Action".Generic(mockedClassType.Name);
                returnType = "ISetup".Generic(mockedClassType.Name);
            }
            else
            {
                returnType = "ISetup".Generic(mockedClassType.Name, method.ReturnType.Name);
                callback = "Func".Generic(mockedClassType.Name, method.ReturnType.Name);
            }

            List<ParameterSyntax> parameters = method.Parameters
                .Select(parameter => Parameter(Identifier(parameter.Name))
                        .WithType(NullableType("Value".Generic(parameter.Type.Name))) // Value<>?
                        .WithDefault(EqualsValueClause(LiteralExpression(SyntaxKind.NullLiteralExpression))) // = null
                )
                .ToList();


            VariableDeclaratorSyntax variableDeclarator = CreateMethodCallExpressionVariableDeclarator(generatedMock.ParameterName, method);

            ExpressionSyntax patchedExpression = Cyber_CretePatchedExpression(method, variableDeclarator.Identifier.Name());

            // return <mock>.Setup();
            ExpressionSyntax returnValue =
                generatedMock.MockVariableName.AccessMember("Setup").Invoke(variableDeclarator.Identifier.Name());

            MethodDeclarationSyntax methodDeclarationSyntax =
                MethodDeclaration(returnType, $"Setup_{generatedMock.ParameterName}_{method.Name}")
                    .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)))
                    .WithParameterList(ParameterList(SeparatedList(parameters)))
                    .WithBody(Block(
                        LocalDeclarationStatement("Expression".Generic(callback).DeclareVariable(variableDeclarator)),
                        IdentifierName(variableDeclarator.Identifier).Assign(value: patchedExpression).ToStatement(),
                        ReturnStatement(returnValue).WithSemicolonToken(SemicolonToken))
                    );
            methods.Add(methodDeclarationSyntax.NormalizeWhitespace());
        }

        return methods;
    }

    private static VariableDeclaratorSyntax CreateMethodCallExpressionVariableDeclarator(string parameterName, IMethodSymbol method)
    {
        List<InvocationExpressionSyntax> allParameterTypesFilled = method.Parameters
            .Select(parameter => Cyber_Fill(parameter.Type.Name))
            .ToList();

        return VariableDeclarator("expression").WithInitializer(
            SimpleLambdaExpression(
                Parameter(Identifier(parameterName)),
                parameterName.AccessMember(method.Name).Invoke(allParameterTypesFilled)
            )
        );
    }

    private static ExpressionSyntax Cyber_CretePatchedExpression(IMethodSymbol method, IdentifierNameSyntax variableName)
    {
        // Cyber.UpdateExpressionWithParameters<T>(expression, <arguments>);
        return "Cyber".AccessMember("UpdateExpressionWithParameters").Invoke(
            variableName,
            method.Parameters
                .Select(parameter => Cyber_CreateExpressionFor_AnyIfNull(parameter.Name, parameter.Type.Name))
                .ArrayInitializer()
                .ImplicitCreation()
        );
    }

    private static InvocationExpressionSyntax Cyber_CreateExpressionFor_AnyIfNull(string name, string type)
    {
        return "Cyber".AccessMember("CreateExpressionFor").Invoke(IdentifierName(name).Coalesce(ValueAny(type)));
    }


    private static InvocationExpressionSyntax Cyber_Fill(string type) => "Cyber".AccessMember("Fill".Generic(type)).Invoke();
    private static MemberAccessExpressionSyntax ValueAny(string type) => "Value".Generic(type).AccessMember("Any");
}