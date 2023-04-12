namespace TestsHelper.SourceGenerator.MockWrapping.Values;

internal sealed record ExactValue<T>(T Value) : Value<T>;