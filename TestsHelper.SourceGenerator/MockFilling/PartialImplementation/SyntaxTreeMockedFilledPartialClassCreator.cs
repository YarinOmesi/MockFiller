using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using TestsHelper.SourceGenerator.FluentSyntaxCreation;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation;

// TODO: rewrite
public class SyntaxTreeMockedFilledPartialClassCreator : IMockedFilledPartialClassCreator
{
    private static readonly SyntaxToken SemicolonToken = Token(SyntaxKind.SemicolonToken);

    private readonly HashSet<string> _usingNamespaces = new();
    private readonly List<Mock> _mocks = new();
    private readonly List<ValueForParameter> _valueForParameters = new();
    private string? _className;
    private string? _namespace;
    private IMethodSymbol? _selectedConstructor;


    public void SetClass(ClassDeclarationSyntax declarationSyntax)
    {
        _className = declarationSyntax.Identifier.Text;
        if (declarationSyntax.Parent is BaseNamespaceDeclarationSyntax parentNamespace)
        {
            _namespace = parentNamespace.Name.ToString();
        }
    }

    public void SetSelectedConstructor(IMethodSymbol selectedConstructor)
    {
        _selectedConstructor = selectedConstructor;
    }

    public void AddMockForType(ITypeSymbol typeSymbol, string parameterName)
    {
        // TODO: Test Case The type is in global namespace (NoNamespace) 
        // TODO: Test Case The Type is in some namespace
        if (typeSymbol.ContainingNamespace.IsGlobalNamespace == false)
        {
            _usingNamespaces.Add(GetNamespace(typeSymbol));
        }

        _mocks.Add(new Mock(typeSymbol, parameterName));
        _usingNamespaces.Add("Moq");
    }

    public void AddValueForParameter(string name, string parameterName)
    {
        _valueForParameters.Add(new ValueForParameter(name, parameterName));
    }

    private static string GetNamespace(ITypeSymbol symbol)
    {
        List<string> namespaces = new();

        INamespaceSymbol @namespace = symbol.ContainingNamespace;
        while (@namespace.IsGlobalNamespace == false)
        {
            namespaces.Add(@namespace.Name);
            @namespace = @namespace.ContainingNamespace;
        }

        namespaces.Reverse();
        return string.Join(".", namespaces);
    }

    private MethodDeclarationSyntax CreatedBuildInstanceMethod(GeneratedMock[] generatedMocks)
    {
        INamedTypeSymbol containingType = _selectedConstructor!.ContainingType;

        IdentifierNameSyntax objectToBuild = IdentifierName(containingType.Name);
        // private <TestedClass> Build()
        MethodDeclarationSyntax method = MethodDeclaration(objectToBuild, "Build")
            .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)));


        Dictionary<string, ExpressionSyntax> argumentsByName = new();

        foreach (GeneratedMock generatedMock in generatedMocks)
        {
            argumentsByName[generatedMock.ParameterName] = generatedMock.MockVariableName.AccessMember("Object");
        }

        foreach (ValueForParameter valueForParameter in _valueForParameters)
        {
            argumentsByName[valueForParameter.ParameterName] = IdentifierName(valueForParameter.Name);
        }

        List<ArgumentSyntax> arguments = _selectedConstructor!.Parameters
            .Select(parameter => Argument(argumentsByName[parameter.Name]))
            .ToList();

        // new TestedClass(arguments)
        ObjectCreationExpressionSyntax testedClassCreating = ObjectCreationExpression(objectToBuild)
            .WithArgumentList(ArgumentList(SeparatedList(arguments)));

        List<StatementSyntax> body = new();

        foreach (GeneratedMock generatedMock in generatedMocks)
        {
            // _mock = new Mock<>();
            StatementSyntax statement = ExpressionStatement(AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                IdentifierName(generatedMock.MockVariableName),
                ObjectCreationExpression(generatedMock.MockVariableType).WithArgumentList(ArgumentList())
            ));
            body.Add(statement);
        }

        // return <testedClassCreating>;
        body.Add(ReturnStatement(testedClassCreating).WithSemicolonToken(SemicolonToken));
        method = method.WithBody(Block(body));

        return method;
    }

    public SourceText Build()
    {
        //TODO: check that namespace not null
        //TODO: check that classname not null
        GeneratedMock[] mockFields = _mocks.Select(GenerateMock).ToArray();
        ClassDeclarationSyntax classDeclarationSyntax = ClassDeclaration(Identifier(_className!))
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.PartialKeyword)))
            .AddMembers(mockFields.Select(mock => (MemberDeclarationSyntax) mock.FieldDeclarationSyntax).ToArray())
            .AddMembers(CreatedBuildInstanceMethod(mockFields));

        // Remove Usings to current namespace
        if (_namespace is not null)
        {
            _usingNamespaces.Remove(_namespace);
        }


        CompilationUnitSyntax cyberCompilation = CSharpSyntaxTree.ParseText(CyberClass).GetCompilationUnitRoot();

        foreach (string cyberUsing in CyberUsings)
        {
            _usingNamespaces.Add(cyberUsing);
        }

        // Add all setup mock methods
        classDeclarationSyntax = classDeclarationSyntax.AddMembers(mockFields.SelectMany(CreateSetupsForClass).ToArray());

        classDeclarationSyntax = classDeclarationSyntax.AddMembers(cyberCompilation.Members.ToArray());
        CompilationUnitSyntax compilationUnitSyntax = CompilationUnit()
            .AddUsings(_usingNamespaces.Select(@namespace => UsingDirective(ParseName(@namespace))).ToArray())
            .AddMembers(NamespaceDeclaration(ParseName(_namespace!)).AddMembers(classDeclarationSyntax))
            .NormalizeWhitespace(eol:Environment.NewLine);

        return SourceText.From(compilationUnitSyntax.ToFullString(), Encoding.UTF8);
    }

    private static IReadOnlyList<MemberDeclarationSyntax> CreateSetupsForClass(GeneratedMock mock)
    {
        List<MethodDeclarationSyntax> methods = new();

        ITypeSymbol mockedClassType = mock.Mock.Type;
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


            VariableDeclaratorSyntax variableDeclarator = CreateMethodCallExpressionVariableDeclarator(mock.ParameterName, method);

            ExpressionSyntax patchedExpression = Cyber_CretePatchedExpression(method, variableDeclarator.Identifier.Name());

            // return <mock>.Setup();
            ExpressionSyntax returnValue = mock.MockVariableName.AccessMember("Setup").Invoke(variableDeclarator.Identifier.Name());

            MethodDeclarationSyntax methodDeclarationSyntax = MethodDeclaration(returnType, $"Setup_{mock.ParameterName}_{method.Name}")
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
                .Select(parameter=> Cyber_CreateExpressionFor_AnyIfNull(parameter.Name,parameter.Type.Name))
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

    private static FieldDeclarationSyntax CreateMockField(Mock mock)
    {
        GenericNameSyntax type = GenericName(
            Identifier("Mock"),
            TypeArgumentList(SingletonSeparatedList<TypeSyntax>(IdentifierName(mock.Type.Name)))
        );

        VariableDeclarationSyntax declaration = VariableDeclaration(type)
            .AddVariables(VariableDeclarator($"_{mock.ParameterName}Mock"));

        return FieldDeclaration(declaration)
            .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)))
            .WithSemicolonToken(SemicolonToken);
    }

    private static GeneratedMock GenerateMock(Mock mock) => new(mock, CreateMockField(mock));

    private readonly record struct GeneratedMock(Mock Mock, FieldDeclarationSyntax FieldDeclarationSyntax)
    {
        public Mock Mock { get; } = Mock;
        public FieldDeclarationSyntax FieldDeclarationSyntax { get; } = FieldDeclarationSyntax;

        public string ParameterName => Mock.ParameterName;

        public string MockVariableName => FieldDeclarationSyntax.Declaration.Variables[0].Identifier.Text;
        public TypeSyntax MockVariableType => FieldDeclarationSyntax.Declaration.Type;
    }

    private readonly record struct Mock(ITypeSymbol Type, string ParameterName)
    {
        public ITypeSymbol Type { get; } = Type;
        public string ParameterName { get; } = ParameterName;
    }

    private readonly record struct ValueForParameter(string Name, string ParameterName)
    {
        public string Name { get; } = Name;
        public string ParameterName { get; } = ParameterName;
    }
}