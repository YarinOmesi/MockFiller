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

    public SetupMethodResult Create(IEnumerable<GeneratedMock> generatedMocks)
    {
        CompilationUnitSyntax cyberCompilation = CSharpSyntaxTree.ParseText(CyberClass).GetCompilationUnitRoot();

        List<MemberDeclarationSyntax> members = new();

        // Add all setup mock methods
        members.AddRange(generatedMocks.SelectMany(CreateForMock));

        members.AddRange(cyberCompilation.Members);

        return new SetupMethodResult(CyberUsings, members);
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

    private static readonly string[] CyberUsings = new[] {
        "Moq",
        "Moq.Language.Flow",
        "System.Linq.Expressions",
        "System",
        "System.Collections.Generic",
        "System.Linq"
    };

    private const string CyberClass = @"
public static class Cyber
{
    // This Method Should Not Be Ran It Used As A Filler For Types
    public static T Fill<T>() => throw new NotImplementedException();

    public static Expression<T> UpdateExpressionWithParameters<T>(Expression<T> expression, IEnumerable<Expression> arguments)
    {
        MethodCallExpression body = (MethodCallExpression) expression.Body;
        return expression.Update(body.Update(body.Object, arguments), expression.Parameters);
    }

    public static Expression CreateExpressionFor<T>(Value<T> value)
    {
        Expression<Func<T>> isAny = () => It.IsAny<T>();
        Expression<Func<T>> itIs = () => It.Is<T>(Fill<T>(), EqualityComparer<T>.Default);
        MethodCallExpression body = (MethodCallExpression) itIs.Body;

        if (value.IsAny)
        {
            return isAny.Body;
        }
        
        List<Expression> newArguments = body.Arguments.ToList();
        newArguments[0] = Expression.Constant(value.IsDefault ? default : value._Value);

        return body.Update(body.Object, newArguments);
    }
}
public readonly struct Value<T>
{
    public static readonly Value<T> Any = new(default, isAny: true);
    public static readonly Value<T> Default = new(default, isDefault: true);
    
    public T _Value { get; }
    public bool IsAny { get; }
    public bool IsDefault { get; }


    public Value(T value, bool isDefault = false, bool isAny = false)
    {
        _Value = value;
        IsDefault = isDefault;
        IsAny = isAny;
    }

    public static implicit operator Value<T>(T value) => new(value);
}";
}

public readonly record struct SetupMethodResult(string[] Usings, IReadOnlyList<MemberDeclarationSyntax> MemberDeclarations)
{
    public string[] Usings { get; } = Usings;
    public IReadOnlyList<MemberDeclarationSyntax> MemberDeclarations { get; } = MemberDeclarations;
}