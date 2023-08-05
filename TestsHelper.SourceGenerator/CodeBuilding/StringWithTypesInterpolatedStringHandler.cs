using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TestsHelper.SourceGenerator.CodeBuilding.Types;

namespace TestsHelper.SourceGenerator.CodeBuilding;

[InterpolatedStringHandler]
public readonly ref struct StringWithTypesInterpolatedStringHandler
{
    public StringWithTypes StringWithTypes { get; }

    public StringWithTypesInterpolatedStringHandler(int literalLength, int formattedCount)
    {
        StringWithTypes = StringWithTypes.Create(literalLength + formattedCount);
    }

    public void AppendLiteral(string s) => StringWithTypes.Add(s);

    public void AppendFormatted<T>(T t) where T : IType => StringWithTypes.Add(t);

    public void AppendFormatted(FieldBuilder field) => StringWithTypes.Add(field.Name);

    public void AppendFormatted(ParameterBuilder parameterBuilder) => StringWithTypes.Add(parameterBuilder.Name);

    public void AppendFormatted(TypeBuilder typeBuilder) => StringWithTypes.Add(typeBuilder.Name);

    public void AppendFormatted(StringWithTypes stringWithTypes) => StringWithTypes.Add(stringWithTypes);

    public void AppendFormatted(IEnumerable<string> array, string format) => StringWithTypes.Add(string.Join(format, array));

    public void AppendFormatted(IEnumerable<StringWithTypes> stringWithTypes, string format)
    {
        StringWithTypes.Add(new StringWithTypes.MultipleValues<StringWithTypes>(stringWithTypes.ToArray(), format));
    }

    public void AppendFormatted(string s) => StringWithTypes.Add(s);

    internal string GetFormattedText() => string.Empty;
}