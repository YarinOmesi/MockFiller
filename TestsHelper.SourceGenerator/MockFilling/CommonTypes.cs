using TestsHelper.SourceGenerator.CodeBuilding.Types;

namespace TestsHelper.SourceGenerator.MockFilling;

public static class CommonTypes
{
    public static readonly string SystemLinqExpressions = "System.Linq.Expressions";
    public static readonly NamespacedType ConverterType = "TestsHelper.SourceGenerator.MockWrapping.Converters".Type("IValueConverter");
    public static readonly NamespacedType ValueType = "TestsHelper.SourceGenerator.MockWrapping".Type("Value");
}