using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace TestsHelper.SourceGenerator.Tests;

/// <summary>
/// this copied from roslyn docs and change to fit incremental source generator
/// <see cref="https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md#unit-testing-of-generators"/>
/// </summary>
/// <typeparam name="TIncrementalSourceGenerator"></typeparam>
public class Test<TIncrementalSourceGenerator> : SourceGeneratorTest<NUnitVerifier>
    where TIncrementalSourceGenerator : IIncrementalGenerator, new()
{
    public LanguageVersion LanguageVersion { get; set; } = LanguageVersion.Default;

    protected override string DefaultFileExt => "cs";

    public override string Language => LanguageNames.CSharp;

    protected override IEnumerable<ISourceGenerator> GetSourceGenerators() => new[] {new TIncrementalSourceGenerator().AsSourceGenerator()};

    protected override GeneratorDriver CreateGeneratorDriver(Project project, ImmutableArray<ISourceGenerator> sourceGenerators)
    {
        return CSharpGeneratorDriver.Create(
            sourceGenerators,
            project.AnalyzerOptions.AdditionalFiles,
            (CSharpParseOptions) project.ParseOptions!,
            project.AnalyzerOptions.AnalyzerConfigOptionsProvider);
    }

    protected override CompilationOptions CreateCompilationOptions()
    {
        var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true);
        return compilationOptions
            .WithSpecificDiagnosticOptions(compilationOptions.SpecificDiagnosticOptions.SetItems(GetNullableWarningsFromCompiler()));
    }

    private static ImmutableDictionary<string, ReportDiagnostic> GetNullableWarningsFromCompiler()
    {
        string[] args = {"/warnaserror:nullable"};
        var commandLineArguments = CSharpCommandLineParser.Default.Parse(args, baseDirectory: Environment.CurrentDirectory,
            sdkDirectory: Environment.CurrentDirectory);
        var nullableWarnings = commandLineArguments.CompilationOptions.SpecificDiagnosticOptions;

        return nullableWarnings;
    }

    protected override ParseOptions CreateParseOptions() => new CSharpParseOptions(LanguageVersion, DocumentationMode.Diagnose);
}