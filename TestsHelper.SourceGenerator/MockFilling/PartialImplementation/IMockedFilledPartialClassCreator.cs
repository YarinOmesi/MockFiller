using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation;

public interface IMockedFilledPartialClassCreator
{
    public void SetClassInfo(ClassDeclarationSyntax declarationSyntax, IMethodSymbol selectedConstructor);
    public void AddMockForType(ITypeSymbol typeSymbol, string parameterName);
    public void AddValueForParameter(string name, string parameterName);
    public SourceText Build();
}