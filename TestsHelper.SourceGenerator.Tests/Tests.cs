using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TestsHelper.SourceGenerator.Attributes;
using TestsHelper.SourceGenerator.Diagnostics;
using TestsHelper.SourceGenerator.MockWrapping;
using TestsHelper.SourceGenerator.SourceGeneratorImplementations;

namespace TestsHelper.SourceGenerator.Tests;

[TestFixture]
public class Tests : TestSuite<IncrementalMockFillerSourceGenerator>
{
    public Tests()
    {
        References.AddRange(new[] {
            MetadataReference.CreateFromFile(typeof(FillMocksAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(FillMocksWithWrappersAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Mock).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ILoggerFactory).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Assert).Assembly.Location),
        });
        IgnoredDiagnostics.Add("CS0169");
        IgnoredDiagnostics.Add("CS0414");
        IgnoredDiagnostics.Add("CS8019");
    }

    [Test]
    public void OnGivenTestedClassWithFillMocksWithWrappersAttribute_GenerateWrappers()
    {
        // Arrange + Act
        GeneratorDriverRunResult result = RunGenerator("GenerateWrappers");

        // Assert
        Assert.That(result.Diagnostics, Is.Empty);
    }

    [Test]
    public void OnGivenTestedClassWithFillMocksAttribute_GenerateMocks()
    {
        // Arrange + Act
        GeneratorDriverRunResult result = RunGenerator("GenerateMocks");

        // Assert
        Assert.That(result.Diagnostics, Is.Empty);
    }

    [Test]
    public void OnTestClassNotPartial_DoNotGenerate()
    {
        // Arrange + Act
        GeneratorDriverRunResult result = RunGenerator("TestClassNotPartial");

        // Assert
        Assert.That(result.Diagnostics, Is.Empty);
    }

    [Test]
    public void OnMoreThanOneFillMocksUsage_ReportDiagnostic()
    {
        // Arrange + Act
        GeneratorDriverRunResult result = RunGenerator("TestMoreThanOneFillMocksUsage");

        // Assert
        Assert.That(result.Diagnostics.Length, Is.EqualTo(1));
        Assert.That(result.Diagnostics[0].Descriptor, Is.EqualTo(DiagnosticRegistry.MoreThanOneFillMockUsage));
    }

    [Test]
    public void DefaultValue_GivenWrongTypeExistingParameter_ReportDiagnostic()
    {
        // Arrange + Act
        GeneratorDriverRunResult result = RunGenerator("WrongTypeDefaultValue");

        // Assert
        Assert.That(result.Diagnostics.Length, Is.EqualTo(1));
        Assert.That(result.Diagnostics[0].Descriptor, Is.EqualTo(DiagnosticRegistry.DefaultValueWithWrongType));
    }

    [Test]
    public void DefaultValue_GivenNotExistingParameter_ReportDiagnostic()
    {
        // Arrange + Act
        GeneratorDriverRunResult result = RunGenerator("NotExistsDefaultValue");

        // Assert
        Assert.That(result.Diagnostics.Length, Is.EqualTo(1));
        Assert.That(result.Diagnostics[0].Descriptor, Is.EqualTo(DiagnosticRegistry.DefaultValueToUnknownParameter));
    }
}