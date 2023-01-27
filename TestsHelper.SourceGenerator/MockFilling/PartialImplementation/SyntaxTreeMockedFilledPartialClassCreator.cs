using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Logics;
using TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Models;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation;

public class SyntaxTreeMockedFilledPartialClassCreator : IMockedFilledPartialClassCreator
{
    private readonly BuildMethodCreator _buildMethodCreator = new BuildMethodCreator();
    private readonly MockGenerator _mockGenerator = new MockGenerator();
    private readonly WrappingMockMethodCreator _wrappingMockMethodCreator = new WrappingMockMethodCreator();


    private readonly HashSet<string> _usingNamespaces = new();
    private readonly List<Mock> _mocks = new();
    private readonly List<ValueForParameter> _valueForParameters = new();
    private WorkingClassInfo? _workingClassInfo;
    private bool _generateMockWrappers = false;


    public void SetClassInfo(ClassDeclarationSyntax declarationSyntax, IMethodSymbol selectedConstructor)
    {
        string @namespace = declarationSyntax.Parent is BaseNamespaceDeclarationSyntax parentNamespace
            ? parentNamespace.Name.ToString()
            : string.Empty;

        _workingClassInfo = new WorkingClassInfo(@namespace, declarationSyntax.Identifier.Text, selectedConstructor);
    }

    public void SetGenerateMockWrapper(bool generate)
    {
        _generateMockWrappers = generate;
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

    public SourceText Build()
    {
        //TODO: check that namespace not null
        //TODO: check that classname not null
        WorkingClassInfo classInfo = _workingClassInfo!.Value;

        GeneratedMock[] mockFields = _mocks.Select(_mockGenerator.Generate).ToArray();
        ClassDeclarationSyntax classDeclarationSyntax = ClassDeclaration(Identifier(classInfo.Name))
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.PartialKeyword)))
            .AddMembers(mockFields.Select(mock => (MemberDeclarationSyntax) mock.FieldDeclarationSyntax).ToArray())
            .AddMembers(_buildMethodCreator.Create(classInfo, mockFields, _valueForParameters));

        // Remove Usings to current namespace
        if (classInfo.Namespace != string.Empty)
        {
            _usingNamespaces.Remove(classInfo.Namespace);
        }


        if (_generateMockWrappers)
        {
            WrapMockMethodResult wrapMockMethodResult = _wrappingMockMethodCreator.Create(mockFields);
            foreach (string cyberUsing in wrapMockMethodResult.Usings)
            {
                _usingNamespaces.Add(cyberUsing);
            }

            // Add all members from wrapping method result
            classDeclarationSyntax = classDeclarationSyntax.AddMembers(wrapMockMethodResult.MemberDeclarations.ToArray());
        }

        CompilationUnitSyntax compilationUnitSyntax = CompilationUnit()
            .AddUsings(_usingNamespaces.Select(@namespace => UsingDirective(ParseName(@namespace))).ToArray())
            .AddMembers(NamespaceDeclaration(ParseName(classInfo.Namespace)).AddMembers(classDeclarationSyntax))
            .NormalizeWhitespace(eol: Environment.NewLine);

        return SourceText.From(compilationUnitSyntax.ToFullString(), Encoding.UTF8);
    }
}