namespace TestsHelper.SourceGenerator.MockWrapping;

public readonly struct Value<T>
{
    public static readonly Value<T> Any = new(default, isAny: true);
    public static readonly Value<T> Default = new(default, isDefault: true);
    
    public T ActualValue { get; }
    public bool IsAny { get; }
    public bool IsDefault { get; }


    private Value(T actualValue, bool isDefault = false, bool isAny = false)
    {
        ActualValue = actualValue;
        IsDefault = isDefault;
        IsAny = isAny;
    }

    public static implicit operator Value<T>(T value) => new(value);
}