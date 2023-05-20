using System;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;
using TestsHelper.SourceGenerator.Diagnostics;
using TestsHelper.SourceGenerator.SourceGeneratorImplementations;
using VerifyCS =
    TestsHelper.SourceGenerator.Tests.CSharpSourceGeneratorVerifier<
        TestsHelper.SourceGenerator.SourceGeneratorImplementations.MockFillerSourceGenerator>;

namespace TestsHelper.SourceGenerator.Tests;

[TestFixture]
public class MockFillerSourceGeneratorTests
{
    private ImmutableArray<string> _referencedAssemblies;
    private ImmutableArray<PackageIdentity> _referencedPackages;
    private DirectoryInfo _currentDirectoryInfo = null!;
#if DEBUG
    private const string Configuration = "Debug";
#else
    private const string Configuration = "Release";
#endif

    [OneTimeSetUp]
    public void Setup()
    {
        string projectName = GetType().Assembly.GetName().Name!;
        var directoryInfo = new DirectoryInfo(Environment.CurrentDirectory);
        while (directoryInfo!.Name != projectName)
        {
            directoryInfo = directoryInfo.Parent;
        }

        _currentDirectoryInfo = directoryInfo.Parent!;
        _referencedAssemblies = ImmutableArray.Create<string>(
            $"{_currentDirectoryInfo.FullName}/TestsHelper.SourceGenerator.Attributes/bin/{Configuration}/netstandard2.0/TestsHelper.SourceGenerator.Attributes",
            $"{_currentDirectoryInfo.FullName}/TestsHelper.SourceGenerator.MockWrapping/bin/{Configuration}/netstandard2.0/TestsHelper.SourceGenerator.MockWrapping"
        );
        string referencedAssemblyPath = _referencedAssemblies[0] + ".dll";
        if (!File.Exists(referencedAssemblyPath))
        {
            throw new Exception($"Attributes Not Exists In Path {referencedAssemblyPath}");
        }

        _referencedPackages = ImmutableArray.Create<PackageIdentity>(
            new PackageIdentity("Microsoft.Extensions.Logging.Abstractions", "7.0.0"),
            new PackageIdentity("Moq", "4.18.4"),
            new PackageIdentity("NUnit", "3.13.3")
        );
    }

    [Test]
    public async Task TestHappyFlow_OnValidClass_GenerateImplementation()
    {
        // Arrange
        var test = new VerifyCS.Test {
            LanguageVersion = LanguageVersion.CSharp10,
            TestState = {
                ReferenceAssemblies = ReferenceAssemblies.Default
                    .AddAssemblies(_referencedAssemblies)
                    .AddPackages(_referencedPackages),
                Sources = {
                    CreateSource("Sources/IDependency.cs"),
                    CreateSource("Sources/ATestFixture.cs"),
                    CreateSource("Sources/TestedClass.cs"),
                },
                GeneratedSources = {
                    CreateExpectedSource<MockFillerSourceGenerator>("Sources/ATestFixture.FilledMock.generated.cs"),
                    CreateExpectedSource<MockFillerSourceGenerator>("Sources/Wrapper.IDependency.generated.cs")
                }
            }
        };
        // Act + Assert
        await test.RunAsync();
    }

    [Test]
    public async Task TestHappyFlow_OnValidClassWithGenerateMockWrappersAttribute_GenerateImplementationAndWrappers()
    {
        // Arrange
        var generateMockProjectPath = $"{_currentDirectoryInfo.FullName}/TestsHelper.SourceGenerator.MockWrapping/bin/{Configuration}/netstandard2.0/TestsHelper.SourceGenerator.MockWrapping";
        var test = new VerifyCS.Test {
            LanguageVersion = LanguageVersion.CSharp10,
            TestState = {
                ReferenceAssemblies = ReferenceAssemblies.Default
                    .AddAssemblies(_referencedAssemblies)
                    .AddAssemblies(ImmutableArray.Create(generateMockProjectPath))
                    .AddPackages(_referencedPackages),
                Sources = {
                    CreateSource("Sources/IDependency.cs"),
                    CreateSource("Sources/ATestFixture_WithGenerateMocks.cs", overrideFileName: "ATestFixture.cs"),
                    CreateSource("Sources/TestedClass.cs"),
                },
                GeneratedSources = {
                    CreateExpectedSource<MockFillerSourceGenerator>(
                        path: "Sources/ATestFixture.FilledMock.WithWrappers.generated.cs",
                        overrideFileName: "ATestFixture.FilledMock.generated.cs"
                    ),
                    CreateExpectedSource<MockFillerSourceGenerator>(
                        path: "Sources/Wrapper.IDependency.WithWrappers.generated.cs",
                        overrideFileName: "Wrapper.IDependency.generated.cs"
                    ),
                }
            }
        };
        // Act + Assert
        await test.RunAsync();
    }

    [Test]
    public async Task ClassNotPartial_DoNotGenerate_ReportError()
    {
        // Arrange
        var test = new VerifyCS.Test {
            LanguageVersion = LanguageVersion.CSharp10,
            TestState = {
                ReferenceAssemblies = ReferenceAssemblies.Default
                    .AddAssemblies(_referencedAssemblies)
                    .AddPackages(_referencedPackages),
                Sources = {
                    CreateSource("Sources/IDependency.cs"),
                    CreateSource("Sources/ATestFixture_NotPartial.cs", overrideFileName: "ATestFixture.cs"),
                    CreateSource("Sources/TestedClass.cs"),
                },
                GeneratedSources = { },
                ExpectedDiagnostics = {
                    (DiagnosticResult.CompilerError(DiagnosticRegistry.ClassIsNotPartial.Id)
                        .WithLocation("ATestFixture.cs", 8, 14))
                }
            }
        };
        // Act + Assert
        await test.RunAsync();
    }

    [Test]
    public async Task MoreThanOneFillMocksAttributeUsage_DoNotGenerate_ReportError()
    {
        // Arrange
        var test = new VerifyCS.Test {
            LanguageVersion = LanguageVersion.CSharp10,
            TestState = {
                ReferenceAssemblies = ReferenceAssemblies.Default
                    .AddAssemblies(_referencedAssemblies)
                    .AddPackages(_referencedPackages),
                Sources = {
                    CreateSource("Sources/IDependency.cs"),
                    CreateSource("Sources/ATestFixture_MoreThanOneFillMocks.cs",  overrideFileName: "ATestFixture.cs"),
                    CreateSource("Sources/TestedClass.cs"),
                },
                GeneratedSources = { },
                ExpectedDiagnostics = {
                    (DiagnosticResult.CompilerError(DiagnosticRegistry.MoreThanOneFillMockUsage.Id)
                        .WithLocation("ATestFixture.cs", 8, 22))
                }
            }
        };
        // Act + Assert
        await test.RunAsync();
    }


    [Test]
    public async Task DefaultValue_GivenNonExistingParameterName_ReportError()
    {
        // Arrange
        var test = new VerifyCS.Test {
            LanguageVersion = LanguageVersion.CSharp10,
            TestState = {
                ReferenceAssemblies = ReferenceAssemblies.Default
                    .AddAssemblies(_referencedAssemblies)
                    .AddPackages(_referencedPackages),
                Sources = {
                    CreateSource("Sources/IDependency.cs"),
                    CreateSource("Sources/ATestFixture__DefaultValue_NonExistingParameter.cs", overrideFileName: "ATestFixture.cs"),
                    CreateSource("Sources/TestedClass.cs"),
                },
                GeneratedSources = { },
                ExpectedDiagnostics = {
                    (DiagnosticResult.CompilerError(DiagnosticRegistry.DefaultValueToUnknownParameter.Id)
                        .WithArguments("NonExistingParameterName"))
                }
            }
        };
        // Act + Assert
        await test.RunAsync();
    }

    [Test]
    public async Task DefaultValue_GivenWrongTypeExistingParameter_ReportError()
    {
        // Arrange
        var test = new VerifyCS.Test {
            LanguageVersion = LanguageVersion.CSharp10,
            TestState = {
                ReferenceAssemblies = ReferenceAssemblies.Default
                    .AddAssemblies(_referencedAssemblies)
                    .AddPackages(_referencedPackages),
                Sources = {
                    CreateSource("Sources/IDependency.cs"),
                    CreateSource("Sources/ATestFixture_DefaultValue_WrongType.cs", overrideFileName: "ATestFixture.cs"),
                    CreateSource("Sources/TestedClass.cs"),
                },
                GeneratedSources = { },
                ExpectedDiagnostics = {
                    (DiagnosticResult.CompilerError(DiagnosticRegistry.DefaultValueWithWrongType.Id)
                        .WithArguments("Int32", "factory", "ILoggerFactory"))
                }
            }
        };
        // Act + Assert
        await test.RunAsync();
    }

    private static (string Filename, SourceText content) CreateSource(string path, string? overrideFileName = null)
    {
        SourceText sourceText = SourceText.From(File.ReadAllText(path).ReplaceLineEndings(Environment.NewLine), Encoding.UTF8);
        return (overrideFileName ?? Path.GetFileName(path), sourceText);
    }

    private static (Type type, string Filename, SourceText content) CreateExpectedSource<T>(string path, string? overrideFileName = null)
    {
        (string filename, SourceText content) = CreateSource(path, overrideFileName);
        return (typeof(T), filename, content);
    }
}