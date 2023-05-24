using TestsHelper.SourceGenerator.CodeBuilding.Types;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Types;

public static class CommonTypes
{
    public static readonly string SystemLinqExpressions = "System.Linq.Expressions";
    public static readonly NamespacedType LinqExpression = SystemLinqExpressions.Type("Expression");
    
    public static readonly string MockWrappingConverters = "TestsHelper.SourceGenerator.MockWrapping.Converters";
    public static readonly NamespacedType IValueConverter = MockWrappingConverters.Type("IValueConverter");
    public static readonly NamespacedType MoqValueConverter = MockWrappingConverters.Type("MoqValueConverter");
    public static readonly NamespacedType Value = "TestsHelper.SourceGenerator.MockWrapping".Type("Value");

    public static readonly string System = "System";
    public static readonly NamespacedType SystemFunc = System.Type("Func");
    public static readonly NamespacedType SystemAction = System.Type("Action");
    
}