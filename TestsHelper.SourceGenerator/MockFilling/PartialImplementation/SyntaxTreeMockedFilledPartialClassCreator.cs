using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using TestsHelper.SourceGenerator.FluentSyntaxCreation;
using TestsHelper.SourceGenerator.MockFilling.Models;
using TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Logics;
using TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Models;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation;

public class SyntaxTreeMockedFilledPartialClassCreator : IMockedFilledPartialClassCreator
{
    private readonly BuildMethodCreator _buildMethodCreator = new BuildMethodCreator();
    private readonly MockGenerator _mockGenerator = new MockGenerator();


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
        if (typeSymbol.ContainingNamespace.IsGlobalNamespace == false)
        {
            _usingNamespaces.Add(typeSymbol.GetNamespace());
        }

        _mocks.Add(new Mock(typeSymbol, parameterName));
        _usingNamespaces.Add("Moq");
    }

    public void AddValueForParameter(string name, string parameterName)
    {
        _valueForParameters.Add(new ValueForParameter(name, parameterName));
    }


    public IReadOnlyList<FileResult> Build()
    {
        List<FileResult> results = new();
        var typeMockWrapperCreator = new TypeMockWrapperCreator(_generateMockWrappers);

        WorkingClassInfo classInfo = _workingClassInfo!.Value;

        GeneratedMock[] mockFields = _mocks.Select(_mockGenerator.Generate).ToArray();

        List<TypeMockResult> typeMockResults = mockFields.Select(typeMockWrapperCreator.Create).ToList();

        IEnumerable<FieldDeclarationSyntax> classFields = typeMockResults.Select(result =>
            IdentifierName(result.Name)
                .DeclareField($"_{result.ParameterName}")
                .AddModifier(SyntaxKind.PrivateKeyword)
        );
        ClassDeclarationSyntax classDeclarationSyntax = ClassDeclaration(Identifier(classInfo.Name))
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.PartialKeyword)))
            .AddMembers(classFields
                .Cast<MemberDeclarationSyntax>()
                .ToArray()
            )
            .AddMembers(_buildMethodCreator.Create(classInfo, typeMockResults, _valueForParameters));

        // Remove Usings to current namespace
        if (classInfo.Namespace != string.Empty)
        {
            _usingNamespaces.Remove(classInfo.Namespace);
        }

        _usingNamespaces.Add("TestsHelper.SourceGenerator.MockWrapping");
        foreach (TypeMockResult typeMockResult in typeMockResults)
        {
            results.Add(new FileResult(
                $"Wrapper.{typeMockResult.WrappedType.Name}.generated.cs",
                SourceText.From(typeMockResult.CompilationUnitSyntax.ToFullString(), Encoding.UTF8)
            ));
        }

        CompilationUnitSyntax compilationUnitSyntax = CompilationUnit()
            .AddUsings(_usingNamespaces.Select(@namespace => UsingDirective(ParseName(@namespace))).ToArray())
            .AddMembers(NamespaceDeclaration(ParseName(classInfo.Namespace)).AddMembers(classDeclarationSyntax))
            .NormalizeWhitespace(eol: Environment.NewLine);

        results.Add(new FileResult(
            $"{classInfo.Name}.FilledMock.generated.cs",
            SourceText.From(compilationUnitSyntax.ToFullString(), Encoding.UTF8)
        ));
        return results;
    }
}