using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsHelper.SourceGenerator.FluentSyntaxCreation;
using TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Models;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Logics;

public class MockGenerator
{
    public GeneratedMock Generate(Mock mock)
    {
        GenericNameSyntax type = "Mock".Generic(mock.Type.Name);
        return new GeneratedMock(mock, type, mock.Type.GetNamespace());
    }
}