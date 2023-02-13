using Microsoft.CodeAnalysis.CSharp.Syntax;
using MockFiller.SourceGenerator.MockFilling.PartialImplementation.Models;
using MockFiller.SourceGenerator.FluentSyntaxCreation;

namespace MockFiller.SourceGenerator.MockFilling.PartialImplementation.Logics;

public class MockGenerator
{
    public GeneratedMock Generate(Mock mock)
    {
        GenericNameSyntax type = "Mock".Generic(mock.Type.Name);
        return new GeneratedMock(mock, type, mock.Type.GetNamespace());
    }
}