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

        CompilationUnitSyntax compilationUnitSyntax = CompilationUnit()
            .AddUsings(_usingNamespaces.Select(@namespace => UsingDirective(ParseName(@namespace))).ToArray())
            .AddMembers(NamespaceDeclaration(ParseName(_namespace!)).AddMembers(classDeclarationSyntax))
            .NormalizeWhitespace(eol:Environment.NewLine);

        return SourceText.From(compilationUnitSyntax.ToFullString(), Encoding.UTF8);
    }

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