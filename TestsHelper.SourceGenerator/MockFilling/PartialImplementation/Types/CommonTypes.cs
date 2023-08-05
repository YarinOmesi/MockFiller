using TestsHelper.SourceGenerator.CodeBuilding.Types;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Types;

public static class CommonTypes
{
    public static readonly string SystemLinqExpressions = "System.Linq.Expressions";
    public static readonly RegularType LinqExpression = SystemLinqExpressions.Type("Expression");
    
    public static readonly string MockWrappingConverters = "TestsHelper.SourceGenerator.MockWrapping.Converters";
    public static readonly RegularType IValueConverter = MockWrappingConverters.Type("IValueConverter");
    public static readonly RegularType MoqValueConverter = MockWrappingConverters.Type("MoqValueConverter");
    public static readonly RegularType Value = "TestsHelper.SourceGenerator.MockWrapping".Type("Value");

    public static readonly string System = "System";
    public static readonly RegularType SystemFunc = System.Type("Func");
    public static readonly RegularType SystemAction = System.Type("Action");
}