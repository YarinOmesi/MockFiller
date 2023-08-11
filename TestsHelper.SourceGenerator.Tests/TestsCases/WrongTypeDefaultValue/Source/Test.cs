using TestsHelper.SourceGenerator.Attributes;
using TestsHelper.SourceGenerator.MockWrapping;
using TestsHelper.SourceGenerator.Tests.TestsCases.Base;

namespace TestsHelper.SourceGenerator.Tests.TestsCases.WrongTypeDefaultValue.Source;

public partial class Test
{
    [FillMocksWithWrappers] 
    private TestedClass _testedClass = null!;

    [DefaultValue("factory")]
    private readonly int _nullFactory = 66;
}