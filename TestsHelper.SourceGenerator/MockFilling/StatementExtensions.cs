using TestsHelper.SourceGenerator.CodeBuilding;
using TestsHelper.SourceGenerator.CodeBuilding.Types;

namespace TestsHelper.SourceGenerator.MockFilling;

public static class StatementExtensions
{
    public static string New(this IType type, params string[] parameters) => $"new {type.MakeString()}({parameters.JoinToString(", ")})";

    public static string Assign(this FieldBuilder builder, string value) => $"{builder.Name} = {value};";
}