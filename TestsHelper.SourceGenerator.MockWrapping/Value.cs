namespace TestsHelper.SourceGenerator.MockWrapping;

public readonly struct Value<T>
{
    public static readonly Value<T> Any = new(default, isAny: true);
    public static readonly Value<T> Default = new(default, isDefault: true);
    
    public T _Value { get; }
    public bool IsAny { get; }
    public bool IsDefault { get; }


    public Value(T value, bool isDefault = false, bool isAny = false)
    {
        _Value = value;
        IsDefault = isDefault;
        IsAny = isAny;
    }

    public static implicit operator Value<T>(T value) => new(value);
}