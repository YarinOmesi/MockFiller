using Microsoft.CodeAnalysis;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation;

public interface IDependencyInitializationBehavior
{
}

public record PredefinedValueDependencyInitialization(string VariableName) : IDependencyInitializationBehavior;

public record MockDependencyInitialization(ITypeSymbol Type) : IDependencyInitializationBehavior;