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

    protected GeneratorDriverRunResult RunGenerator(string testDirectory)
    {
        CSharpGeneratorDriver generatorDriver = CreateDriver();

        string testCasesDirectoryPath = Path.Join(Environment.CurrentDirectory, TestCasesDirectoryName);
        
        // Copy Test Case Base Classes
        string baseFilesDirectory = Path.Combine(testCasesDirectoryPath, "Base");
        IEnumerable<string> baseFilePaths = Directory.EnumerateFiles(baseFilesDirectory);
        Dictionary<string, string> baseFileNameToContent = baseFilePaths.ToDictionary(path => Path.GetFileName(path)!, File.ReadAllText);

        // Validate Test Directory
        string basePath = Path.Join(testCasesDirectoryPath, testDirectory);
        Assert.That(Directory.Exists(basePath), Is.True, () => $"Base Path Not Exists {basePath}");

        // Validate Sources Exists
        string sourceFilesDirectory = Path.Combine(basePath, "Source");
        IEnumerable<string> sourceFilePaths = Directory.EnumerateFiles(sourceFilesDirectory);
        Dictionary<string, string> inputNameToContent = DictionaryHelper.MergeRight(
            baseFileNameToContent,
            sourceFilePaths.ToDictionary(path => Path.GetFileName(path)!, File.ReadAllText)  
        );
        
        Assert.That(inputNameToContent, Is.Not.Empty, () => $"No Source File Was Found In {sourceFilesDirectory}");

        // Read Output
        string outputFilesDirectory = Path.Combine(basePath, "Output");
        IEnumerable<string> outputFilePaths = Directory.Exists(outputFilesDirectory)
            ? Directory.EnumerateFiles(outputFilesDirectory)
            : Enumerable.Empty<string>();

        Dictionary<string, string> expectedOutputNameToContent =
            outputFilePaths.ToDictionary(path => Path.GetFileName(path)!, File.ReadAllText);

        // Create Compilation
        Compilation compilation = CreateCompilation(inputNameToContent.Values);

        // Run Generator
        GeneratorDriver driver = generatorDriver.RunGenerators(compilation);
        GeneratorDriverRunResult result = driver.GetRunResult();

        // Verify Expected Output Again Actual Output
        Dictionary<string, SourceText> nameToActualOutput = result.GeneratedTrees
            .ToDictionary(tree => Path.GetFileName(tree.FilePath)!, tree => tree.GetText());

        Assert.That(nameToActualOutput.Keys, Is.EquivalentTo(expectedOutputNameToContent.Keys), static () => "Expected File Names");

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

    protected Compilation CreateCompilation(IEnumerable<string> files)
    {
        return CSharpCompilation.Create(
            "compilation",
            files.Select(file => CSharpSyntaxTree.ParseText(file, ParseOptions)),
            References,
            CompilationOptions
        );
    }

    protected CSharpGeneratorDriver CreateDriver()
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