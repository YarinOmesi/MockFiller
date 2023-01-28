using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsHelper.SourceGenerator.MockFilling.Models;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation;

public interface IMockedFilledPartialClassCreator
{
    public void SetClassInfo(ClassDeclarationSyntax declarationSyntax, IMethodSymbol selectedConstructor);
    public void SetGenerateMockWrapper(bool generate);
    public void AddMockForType(ITypeSymbol typeSymbol, string parameterName);
    public void AddValueForParameter(string name, string parameterName);
    public IReadOnlyList<FileResult> Build();
}