using System.Collections.Generic;
using System.Linq;
using System.Text;
using OneOf;
using TestsHelper.SourceGenerator.CodeBuilding.Types;

namespace TestsHelper.SourceGenerator.CodeBuilding;

public sealed class StringWithTypes
{
    public static readonly StringWithTypes Empty = new(EmptyList<OneOf<string, IType, StringWithTypes, MultipleValues<StringWithTypes>>>.Instance);
    
    public bool IsEmpty => this == Empty;
    
    private readonly List<OneOf<string, IType, StringWithTypes, MultipleValues<StringWithTypes>>> _components;

    private StringWithTypes(List<OneOf<string, IType, StringWithTypes, MultipleValues<StringWithTypes>>> components)
    {
        _components = components;
    }

    public void Add(OneOf<string, IType, StringWithTypes, MultipleValues<StringWithTypes>> a) => _components.Add(a);
    public StringWithTypes TakeIf(bool condition) => condition ? this : Empty;

    public string ToString(FileBuilder fileBuilder)
    {
        StringBuilder builder = new StringBuilder();

        foreach (OneOf<string, IType, StringWithTypes, MultipleValues<StringWithTypes>> oneOf in _components)
        {
            string? text = oneOf.Match(
                static s => s,
                type => type.TryRegisterAlias(fileBuilder).MakeString(),
                stringWithTypes => stringWithTypes.IsEmpty ? null : stringWithTypes.ToString(fileBuilder),
                multipleStrings =>
                    string.Join(multipleStrings.Separator, multipleStrings.Values
                        .Where(types => !types.IsEmpty)
                        .Select(types => types.ToString(fileBuilder))
                    )
            );
            if (text != null)
            {
                builder.Append(text);
            }
        }

        return builder.ToString();
    }

    public static StringWithTypes Format(StringWithTypesInterpolatedStringHandler stringHandler) => stringHandler.StringWithTypes;

    public static StringWithTypes Create(int capacity) =>
        new(new List<OneOf<string, IType, StringWithTypes, MultipleValues<StringWithTypes>>>(capacity));

    public readonly record struct MultipleValues<T>(T[] Values, string Separator);
    
    public static implicit operator StringWithTypes(string s)
    {
        StringWithTypes stringWithTypes = Create(1);
        stringWithTypes.Add(s);
        return stringWithTypes;
    }
}