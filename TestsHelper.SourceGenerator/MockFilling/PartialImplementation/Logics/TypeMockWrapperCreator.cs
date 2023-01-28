using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsHelper.SourceGenerator.FluentSyntaxCreation;
using TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Models;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Logics;

public class TypeMockWrapperCreator
{
    private static readonly SyntaxToken SemicolonToken = Token(SyntaxKind.SemicolonToken);
    private static readonly SyntaxToken VarIdentifier = Identifier(TriviaList(), SyntaxKind.VarKeyword, "var", "var", TriviaList());
    private static readonly PredefinedTypeSyntax VoidTypeSyntax = PredefinedType(Token(SyntaxKind.VoidKeyword));
    private static readonly LiteralExpressionSyntax NullValueSyntax = LiteralExpression(SyntaxKind.NullLiteralExpression);

    private static readonly string[] CyberUsings = new[] {
        "TestsHelper.SourceGenerator.MockWrapping",
        "Moq",
        "Moq.Language.Flow",
        "System.Linq.Expressions",
        "System",
        "System.Collections.Generic",
        "System.Linq"
    };

    public TypeMockResult Create(GeneratedMock generatedMock, bool createMockWrapperMethod)
    {
        ClassDeclarationSyntax wrapperClass = ClassDeclaration($"Wrapper_{generatedMock.Mock.Type.Name}")
            .AddModifiers(Token(SyntaxKind.PublicKeyword));

        ParameterSyntax mockParameter = $"{generatedMock.ParameterName}Mock".Parameter(generatedMock.MockVariableType);

        // public Mock<> Mock { get; }
        PropertyDeclarationSyntax mockField = PropertyDeclaration(generatedMock.MockVariableType, "Mock")
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .WithAccessorList(AccessorList(List(new[] {
                AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SemicolonToken)
            })));

        wrapperClass = wrapperClass.AddMembers(mockField);

        List<StatementSyntax> constructorStatements = new() {
            // Mock = mock
            mockField.Identifier.Name().Assign(mockParameter.Identifier.Name()).ToStatement()
        };

        List<ClassDeclarationSyntax> methodWrapperClasses = new();

        if (createMockWrapperMethod)
        {
            ITypeSymbol mockedClassType = generatedMock.Mock.Type;
            List<IMethodSymbol> publicMethods = mockedClassType.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(method => method.DeclaredAccessibility == Accessibility.Public)
                .ToList();

            foreach (IMethodSymbol method in publicMethods)
            {
                // Method_type
                ClassDeclarationSyntax methodWrapper = CreateMethodWrapperClass(generatedMock, method);

                // public Method_type name { get; }
                PropertyDeclarationSyntax methodProperty = PropertyDeclaration(methodWrapper.Identifier.Name(), method.Name)
                    .AddModifiers(Token(SyntaxKind.PublicKeyword))
                    .WithAccessorList(AccessorList(List(new[] {
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SemicolonToken)
                    })));

                wrapperClass = wrapperClass.AddMembers(methodProperty);

                constructorStatements.Add(
                    // new Wrapper_type(new Mock<>)
                    methodProperty.Identifier.Name()
                        .Assign(methodWrapper.Identifier.Name().New(arguments: mockParameter.Identifier.Name()))
                        .ToStatement()
                );
                methodWrapperClasses.Add(methodWrapper);
            }
        }

        // Add ctor
        wrapperClass = wrapperClass.AddMembers(
            ConstructorDeclaration(wrapperClass.Identifier)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(mockParameter)
                .WithBody(Block(constructorStatements))
        );

        wrapperClass = wrapperClass.AddMembers(methodWrapperClasses.Cast<MemberDeclarationSyntax>().ToArray());

        string classNamespace = "TestsHelper.SourceGenerator.MockWrapping";
        CompilationUnitSyntax compilationUnitSyntax = CompilationUnit()
            .AddUsings(CyberUsings.Select(name => UsingDirective(ParseName(name))).ToArray())
            .AddUsings(UsingDirective(ParseName(generatedMock.TypeNamespace)))
            .AddMembers(NamespaceDeclaration(ParseName(classNamespace)).AddMembers(wrapperClass))
            .NormalizeWhitespace(eol: Environment.NewLine);

        return new TypeMockResult(
            Name: wrapperClass.Identifier.Text,
            Namespace: classNamespace,
            GeneratedMock: generatedMock,
            MockPropertyName: mockField.Identifier.Text,
            CompilationUnitSyntax: compilationUnitSyntax
        );
    }

    private ClassDeclarationSyntax CreateMethodWrapperClass(GeneratedMock generatedMock, IMethodSymbol method)
    {
        string expressionFieldName = "_expression";
        ClassDeclarationSyntax methodWrapper = ClassDeclaration($"Method_{method.Name}")
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddMembers(CreateMethodCallExpressionField(generatedMock, method, expressionFieldName));

        string mockVariableName = "_mock";

        // private readonly Mock<> _mock;
        FieldDeclarationSyntax mockField = generatedMock.MockVariableType.DeclareField(mockVariableName)
            .AddModifiers(SyntaxKind.PrivateKeyword, SyntaxKind.ReadOnlyKeyword)
            .WithSemicolonToken(SemicolonToken);

        methodWrapper = methodWrapper.AddMembers(mockField);

        ConstructorDeclarationSyntax constructorDeclarationSyntax = ConstructorDeclaration(methodWrapper.Identifier)
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddParameterListParameters("mock".Parameter(generatedMock.MockVariableType))
            .AddBodyStatements(mockVariableName.Assign("mock").ToStatement());

        methodWrapper = methodWrapper.AddMembers(constructorDeclarationSyntax);


        List<ParameterSyntax> parameters = method.Parameters
            .Select(parameter => parameter.Name
                    .Parameter(NullableType("Value".Generic(parameter.Type.Name))) // Value<>?
                    .WithDefault(EqualsValueClause(NullValueSyntax)) // = null
            )
            .ToList();

        // Setup
        methodWrapper = methodWrapper.AddMembers(CreateSetupMethod(expressionFieldName, generatedMock.Mock.Type, mockVariableName, method, parameters));
        // Verify
        methodWrapper = methodWrapper.AddMembers(CreateVerifyMethod(expressionFieldName, mockVariableName, method, parameters));

        return methodWrapper;
    }

    private MethodDeclarationSyntax CreateSetupMethod(
        string expressionFieldName,
        ITypeSymbol mockedClassType,
        string mockName,
        IMethodSymbol method,
        List<ParameterSyntax> parametersWrappedWithValue)
    {
        GenericNameSyntax setupReturnType = method.ReturnType.SpecialType == SpecialType.System_Void
            ? "ISetup".Generic(mockedClassType.Name)
            : "ISetup".Generic(mockedClassType.Name, method.ReturnType.Name);

        ExpressionSyntax patchedExpression = Cyber_CretePatchedExpression(method, IdentifierName(expressionFieldName));

        string expressionVariableName = "expression";

        ExpressionSyntax setupReturnValue = mockName.AccessMember("Setup").Invoke(IdentifierName(expressionVariableName));

        return MethodDeclaration(setupReturnType, "Setup")
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
            .WithParameterList(ParameterList(SeparatedList(parametersWrappedWithValue)))
            .AddBodyStatements(
                LocalDeclarationStatement(IdentifierName(VarIdentifier).DeclareVariable("expression", initializer: patchedExpression))
            )
            .AddBodyStatements(setupReturnValue.Return());
    }

    private MethodDeclarationSyntax CreateVerifyMethod(
        string expressionFieldName,
        string mockName,
        IMethodSymbol method,
        List<ParameterSyntax> parametersWrappedWithValue)
    {
        ParameterSyntax timesParameter = "times".Parameter(NullableType(IdentifierName("Times")))
            .WithDefault(EqualsValueClause(NullValueSyntax));

        // Times.AtLeastOnce()
        ExpressionSyntax defaultTimes = "Times".AccessMember("AtLeastOnce").Invoke();

        string expressionVariableName = "expression";

        ExpressionSyntax verifyReturnValue = mockName.AccessMember("Verify").Invoke(
            IdentifierName(expressionVariableName),
            timesParameter.Identifier.Name().Coalesce(defaultTimes)
        );

        return MethodDeclaration(VoidTypeSyntax, "Verify")
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
            .WithParameterList(ParameterList(SeparatedList(parametersWrappedWithValue)))
            .AddParameterListParameters(timesParameter)
            .AddBodyStatements(
                LocalDeclarationStatement(IdentifierName(VarIdentifier)
                    .DeclareVariable("expression", initializer: Cyber_CretePatchedExpression(method, IdentifierName(expressionFieldName))))
            )
            .AddBodyStatements(verifyReturnValue.ToStatement());
    }

    private static FieldDeclarationSyntax CreateMethodCallExpressionField(
        GeneratedMock generatedMock,
        IMethodSymbol method,
        string expressionVariableName)
    {
        GenericNameSyntax moqCallbackType = CalculateMoqCallbackType(method, generatedMock.Mock.Type.Name);

        GenericNameSyntax type = "Expression".Generic(moqCallbackType);
        
        // private readonly Expression<callback> name = x => x.method();
        return type.DeclareField(expressionVariableName, CreateMoqExpressionLambda(expressionVariableName, method))
            .AddModifiers(SyntaxKind.PrivateKeyword, SyntaxKind.ReadOnlyKeyword)
            .WithSemicolonToken(SemicolonToken);
    }

    private static SimpleLambdaExpressionSyntax CreateMoqExpressionLambda(string parameterName, IMethodSymbol method)
    {
        List<InvocationExpressionSyntax> allParameterTypesFilled = method.Parameters
            .Select(parameter => Cyber_Fill(parameter.Type.Name))
            .ToList();

        return SimpleLambdaExpression(
            Parameter(Identifier(parameterName)),
            parameterName.AccessMember(method.Name).Invoke(allParameterTypesFilled)
        );
    }

    private static ExpressionSyntax Cyber_CretePatchedExpression(IMethodSymbol method, ExpressionSyntax variableName)
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

    private static GenericNameSyntax CalculateMoqCallbackType(IMethodSymbol method, string mockedClassName)
    {
        return method.ReturnType.SpecialType == SpecialType.System_Void
            ? "Action".Generic(mockedClassName)
            : "Func".Generic(mockedClassName, method.ReturnType.Name);
    }


    private static InvocationExpressionSyntax Cyber_Fill(string type) => "Cyber".AccessMember("Fill".Generic(type)).Invoke();

    private static MemberAccessExpressionSyntax ValueAny(string type) => "Value".Generic(type).AccessMember("Any");
}