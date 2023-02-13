using Microsoft.CodeAnalysis.CSharp.Syntax;
using MockFiller.Analyzers.FluentSyntaxCreation;
using MockFiller.Analyzers.SourceGenerators.MockFilling.PartialImplementation.Models;

namespace MockFiller.Analyzers.SourceGenerators.MockFilling.PartialImplementation.Logics;

public class MockGenerator
{
    public GeneratedMock Generate(Mock mock)
    {
        GenericNameSyntax type = "Mock".Generic(mock.Type.Name);
        return new GeneratedMock(mock, type, mock.Type.GetNamespace());
    }
}