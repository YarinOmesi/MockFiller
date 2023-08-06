using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Basic.Reference.Assemblies;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;

namespace TestsHelper.SourceGenerator.Tests;

public abstract class TestSuite<TSourceGenerator> where TSourceGenerator : IIncrementalGenerator, new()
{
    protected CSharpCompilationOptions CompilationOptions { get; set; } = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
    protected CSharpParseOptions ParseOptions { get; set; } = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp10);
    protected List<MetadataReference> References { get; set; } = new List<MetadataReference>();
    protected HashSet<string> IgnoredDiagnostics { get; } = new HashSet<string>();

    private const string TestCasesDirectoryName = "TestsCases";

    protected TestSuite()
    {
        References.AddRange(Net60.References.All);
    }

    private static Dictionary<string, string> TryReadFilesFromDirOrEmpty(string baseDirectory, string directory)
    {
        string path = Path.Combine(baseDirectory, directory);
        return Directory.Exists(path)
            ? Directory.EnumerateFiles(path).ToDictionary(filePath => Path.GetFileName(filePath)!, File.ReadAllText)
            : new Dictionary<string, string>();
    }

    protected GeneratorDriverRunResult RunGenerator(string testDirectory)
    {
        CSharpGeneratorDriver generatorDriver = CreateDriver();

        string testCasesDirectoryPath = Path.Join(Environment.CurrentDirectory, TestCasesDirectoryName);
        
        // Copy Test Case Base Classes
        Dictionary<string, string> baseFiles = TryReadFilesFromDirOrEmpty(testCasesDirectoryPath, "Base");

        // Validate Test Directory
        string basePath = Path.Join(testCasesDirectoryPath, testDirectory);
        Assert.That(Directory.Exists(basePath), Is.True, () => $"Base Path Not Exists {basePath}");

        // Validate Sources Exists
        Dictionary<string, string> sourceFiles = TryReadFilesFromDirOrEmpty(basePath, "Source");
        Dictionary<string, string> inputNameToContent = baseFiles.Concat(sourceFiles).ToDictionary(pair => pair.Key, pair => pair.Value);

        Assert.That(inputNameToContent, Is.Not.Empty, () => "No Source File Was Found In Source Directory");

        // Read Output
        Dictionary<string, string> expectedOutputNameToContent = TryReadFilesFromDirOrEmpty(basePath, "Output");

        // Create Compilation
        Compilation compilation = CreateCompilation(inputNameToContent.Values);

        // Run Generator
        GeneratorDriver driver = generatorDriver.RunGenerators(compilation);
        GeneratorDriverRunResult result = driver.GetRunResult();

        // Verify Expected Output Again Actual Output
        Dictionary<string, SourceText> nameToActualOutput = result.GeneratedTrees
            .ToDictionary(tree => Path.GetFileName(tree.FilePath)!, tree => tree.GetText());

        Assert.That(nameToActualOutput.Keys, Is.EquivalentTo(expectedOutputNameToContent.Keys), static () => "Expected File Names");

        // Run Differences
        foreach ((string fileName, string expectedContent) in expectedOutputNameToContent)
        {
            SourceText actualContent = nameToActualOutput[fileName];

            Assert.That(actualContent.ToString(), new DiffConstraint(expectedContent));
        }

        Compilation newCompilation = compilation
            .WithOptions(CompilationOptions.WithSpecificDiagnosticOptions(
                IgnoredDiagnostics.ToDictionary(s => s, _ => ReportDiagnostic.Suppress)))
            .AddSyntaxTrees(result.GeneratedTrees);

        Assert.That(newCompilation.GetDiagnostics(), Is.Empty);

        return result;
    }

    private Compilation CreateCompilation(IEnumerable<string> files)
    {
        return CSharpCompilation.Create(
            "compilation",
            files.Select(file => CSharpSyntaxTree.ParseText(file, ParseOptions)),
            References,
            CompilationOptions
        );
    }

    private CSharpGeneratorDriver CreateDriver()
    {
        var sourceGenerator = new TSourceGenerator();

        return CSharpGeneratorDriver.Create(
            generators: new[] {sourceGenerator.AsSourceGenerator()},
            additionalTexts: Enumerable.Empty<AdditionalText>(),
            parseOptions: ParseOptions,
            optionsProvider: null,
            driverOptions: new GeneratorDriverOptions(default)
        );
    }
}