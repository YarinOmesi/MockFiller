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
    private static readonly LiteralExpressionSyntax DefaultValueSyntax = LiteralExpression(SyntaxKind.DefaultLiteralExpression);

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
        ParameterSyntax valueConverterParameter = "converter".Parameter(IdentifierName("IValueConverter"));

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
#pragma warning disable RS1024
            Dictionary<string, IReadOnlyList<IMethodSymbol>> publicMethodsByName = mockedClassType.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(method => method.DeclaredAccessibility == Accessibility.Public)
                .GroupBy<IMethodSymbol, string>(symbol => symbol.Name)
                .ToDictionary(grouping => grouping.Key, grouping => (IReadOnlyList<IMethodSymbol>) grouping.ToList());
#pragma warning restore RS1024


            foreach ((string name, IReadOnlyList<IMethodSymbol> methods) in publicMethodsByName.Select(pair => (pair.Key, pair.Value)))
            {
                // Use The Longest Parameters Method
                IMethodSymbol method = methods.OrderByDescending(symbol => symbol.Parameters.Length).First();
                
                // Method_type
                ClassDeclarationSyntax methodWrapper = CreateMethodWrapperClass(generatedMock, method);

                // public Method_type name { get; }
                PropertyDeclarationSyntax methodProperty = PropertyDeclaration(methodWrapper.Identifier.Name(), name)
                    .AddModifiers(Token(SyntaxKind.PublicKeyword))
                    .WithAccessorList(AccessorList(List(new[] {
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SemicolonToken)
                    })));

                wrapperClass = wrapperClass.AddMembers(methodProperty);

                constructorStatements.Add(
                    // new Wrapper_type(new Mock<>, _converter)
                    methodProperty.Identifier.Name()
                        .Assign(methodWrapper.Identifier.Name()
                            .New(mockParameter.Identifier.Name(), valueConverterParameter.Identifier.Name())
                        )
                        .ToStatement()
                );
                methodWrapperClasses.Add(methodWrapper);
            }
        }

        // Add ctor
        ConstructorDeclarationSyntax constructor = ConstructorDeclaration(wrapperClass.Identifier)
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddParameterListParameters(mockParameter)
            .WithBody(Block(constructorStatements));
        if (createMockWrapperMethod)
        {
            constructor = constructor.AddParameterListParameters(valueConverterParameter);
        }
        wrapperClass = wrapperClass.AddMembers(constructor);

        wrapperClass = wrapperClass.AddMembers(methodWrapperClasses.Cast<MemberDeclarationSyntax>().ToArray());

        string classNamespace = "TestsHelper.SourceGenerator.MockWrapping";
        List<UsingDirectiveSyntax> usings = CyberUsings.Select(name => UsingDirective(ParseName(name))).ToList();
        if (createMockWrapperMethod)
        {
            usings.Add(UsingDirective(ParseName("TestsHelper.SourceGenerator.MockWrapping.Converters")));
        }

        CompilationUnitSyntax compilationUnitSyntax = CompilationUnit()
            .AddUsings(usings.ToArray())
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

        // private readonly IValueConverter _converter;
        string converterFieldName = "_converter";
        FieldDeclarationSyntax valueConvertor = "IValueConverter".DeclareField(converterFieldName)
            .AddModifiers(SyntaxKind.PrivateKeyword, SyntaxKind.ReadOnlyKeyword)
            .WithSemicolonToken(SemicolonToken);

        methodWrapper = methodWrapper.AddMembers(mockField, valueConvertor);

        ConstructorDeclarationSyntax constructorDeclarationSyntax = ConstructorDeclaration(methodWrapper.Identifier)
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddParameterListParameters(
                "mock".Parameter(generatedMock.MockVariableType),
                "converter".Parameter(IdentifierName("IValueConverter"))
            )
            .AddBodyStatements(
                mockVariableName.Assign("mock").ToStatement(), 
                converterFieldName.Assign("converter").ToStatement()
            );

        methodWrapper = methodWrapper.AddMembers(constructorDeclarationSyntax);


        List<ParameterSyntax> parameters = method.Parameters
            .Select(parameter => parameter.Name
                    .Parameter("Value".Generic(parameter.Type.Name)) // Value<>
                    .WithDefault(EqualsValueClause(DefaultValueSyntax)) // = default
            )
            .ToList();

        
        ExpressionSyntax patchedExpression = Cyber_CretePatchedExpression(method, IdentifierName(expressionFieldName), converterFieldName);
        methodWrapper = methodWrapper.AddMembers(
            // Setup
            CreateSetupMethod(patchedExpression, generatedMock.Mock.Type, mockVariableName, method, parameters),
            // Verify
            CreateVerifyMethod(patchedExpression, mockVariableName, parameters)
        );

        return methodWrapper;
    }

    private MethodDeclarationSyntax CreateSetupMethod(
        ExpressionSyntax patchedExpression,
        ITypeSymbol mockedClassType,
        string mockName,
        IMethodSymbol method,
        List<ParameterSyntax> parametersWrappedWithValue)
    {
        GenericNameSyntax setupReturnType = method.ReturnType.SpecialType == SpecialType.System_Void
            ? "ISetup".Generic(mockedClassType.Name)
            : "ISetup".Generic(mockedClassType.Name, method.ReturnType.Name);

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
        ExpressionSyntax patchedExpression,
        string mockName,
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
                    .DeclareVariable("expression", initializer: patchedExpression))
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

    private static ExpressionSyntax Cyber_CretePatchedExpression(IMethodSymbol method, ExpressionSyntax variableName, string converterFieldName)
    {
        // Cyber.UpdateExpressionWithParameters<T>(expression, <arguments>);
        return "Cyber".AccessMember("UpdateExpressionWithParameters").Invoke(
            variableName,
            method.Parameters
                .Select(parameter => 
                    // _converter.Convert(parameter)
                    converterFieldName.AccessMember("Convert").Invoke(IdentifierName(parameter.Name))
                )
                .ArrayInitializer()
                .ImplicitCreation()
        );
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