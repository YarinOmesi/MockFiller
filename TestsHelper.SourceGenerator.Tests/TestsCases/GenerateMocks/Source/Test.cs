using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TestsHelper.SourceGenerator.Attributes;
using TestsHelper.SourceGenerator.Tests.TestsCases.Base;

namespace TestsHelper.SourceGenerator.Tests.TestsCases.GenerateMocks.Source;

public partial class Test
{
    [FillMocks] 
    private TestedClass _testedClass = null!;

    [DefaultValue("factory")]
    private readonly ILoggerFactory _nullFactory = NullLoggerFactory.Instance;
}