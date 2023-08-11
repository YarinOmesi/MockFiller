using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TestsHelper.SourceGenerator.Attributes;
using TestsHelper.SourceGenerator.MockWrapping;
using TestsHelper.SourceGenerator.Tests.TestsCases.Base;

namespace TestsHelper.SourceGenerator.Tests.TestsCases.GenerateWrappers.Source;

public partial class Test
{
    [FillMocksWithWrappers] 
    private TestedClass _testedClass = null!;

    [DefaultValue("factory")]
    private readonly ILoggerFactory _nullFactory = NullLoggerFactory.Instance;
}