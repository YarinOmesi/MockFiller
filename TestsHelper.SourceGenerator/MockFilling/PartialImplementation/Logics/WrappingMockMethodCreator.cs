using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsHelper.SourceGenerator.FluentSyntaxCreation;
using TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Models;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Logics;

public class WrappingMockMethodCreator
{
    private static readonly SyntaxToken SemicolonToken = Token(SyntaxKind.SemicolonToken);
    private static readonly PredefinedTypeSyntax VoidTypeSyntax = PredefinedType(Token(SyntaxKind.VoidKeyword));

    private static readonly string[] CyberUsings = new[] {
        "TestsHelper.SourceGenerator.MockWrapping",
        "Moq",
        "Moq.Language.Flow",
        "System.Linq.Expressions",
        "System",
        "System.Collections.Generic",
        "System.Linq"
    };

    private static readonly LiteralExpressionSyntax NullValueSyntax = LiteralExpression(SyntaxKind.NullLiteralExpression);

    public WrapMockMethodResult Create(IEnumerable<GeneratedMock> generatedMocks)
    {
        return new WrapMockMethodResult(CyberUsings, generatedMocks.SelectMany(CreateForMock).ToList());
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
            GenericNameSyntax setupReturnType;

            if (method.ReturnType.SpecialType == SpecialType.System_Void)
            {
                callback = "Action".Generic(mockedClassType.Name);
                setupReturnType = "ISetup".Generic(mockedClassType.Name);
            }
            else
            {
                setupReturnType = "ISetup".Generic(mockedClassType.Name, method.ReturnType.Name);
                callback = "Func".Generic(mockedClassType.Name, method.ReturnType.Name);
            }

            List<ParameterSyntax> parameters = method.Parameters
                .Select(parameter => Parameter(Identifier(parameter.Name))
                        .WithType(NullableType("Value".Generic(parameter.Type.Name))) // Value<>?
                        .WithDefault(EqualsValueClause(NullValueSyntax)) // = null
                )
                .ToList();

            // expression = <parameterName> => <parameterName>.<methodName>(Fill<T>()....)  
            VariableDeclaratorSyntax variableDeclarator = CreateMethodCallExpressionVariableDeclarator(generatedMock.ParameterName, method);

            // Fill method call parameters
            ExpressionSyntax patchedExpression = Cyber_CretePatchedExpression(method, variableDeclarator.Identifier.Name());

            StatementSyntax[] expressionBuildingStatements = new StatementSyntax[] {
                LocalDeclarationStatement("Expression".Generic(callback).DeclareVariable(variableDeclarator)),
                IdentifierName(variableDeclarator.Identifier).Assign(value: patchedExpression).ToStatement()
            };

            // <mock>.Setup();
            ExpressionSyntax setupReturnValue =
                generatedMock.MockVariableName.AccessMember("Setup").Invoke(variableDeclarator.Identifier.Name());

            MethodDeclarationSyntax setupMethodDeclaration =
                MethodDeclaration(setupReturnType, $"Setup_{generatedMock.ParameterName}_{method.Name}")
                    .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)))
                    .WithParameterList(ParameterList(SeparatedList(parameters)))
                    .AddBodyStatements(expressionBuildingStatements)
                    .AddBodyStatements(ReturnStatement(setupReturnValue).WithSemicolonToken(SemicolonToken));

            methods.Add(setupMethodDeclaration.NormalizeWhitespace());


            ParameterSyntax timesParameter = Parameter(Identifier("times"))
                .WithType(NullableType(IdentifierName("Times")))
                .WithDefault(EqualsValueClause(NullValueSyntax));
            
            // Times.AtLeastOnce()
            ExpressionSyntax defaultTimes = "Times".AccessMember("AtLeastOnce").Invoke();

            // <mock>.Verify();
            ExpressionSyntax verifyReturnValue = generatedMock.MockVariableName.AccessMember("Verify")
                .Invoke(
                    variableDeclarator.Identifier.Name(),
                    timesParameter.Identifier.Name().Coalesce(defaultTimes)
                );


            MethodDeclarationSyntax verifyMethodDeclaration =
                MethodDeclaration(VoidTypeSyntax, $"Verify_{generatedMock.ParameterName}_{method.Name}")
                    .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)))
                    .WithParameterList(ParameterList(SeparatedList(parameters)))
                    .AddParameterListParameters(timesParameter)
                    .AddBodyStatements(expressionBuildingStatements)
                    .AddBodyStatements(verifyReturnValue.ToStatement());
            methods.Add(verifyMethodDeclaration.NormalizeWhitespace());
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