using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using TestsHelper.SourceGenerator.Attributes;
using TestsHelper.SourceGenerator.MockWrapping;
using TestsHelper.SourceGenerator.Tests.TestsCases.Base;

namespace TestsHelper.SourceGenerator.Tests.TestsCases.NotExistsDefaultValue.Source;

public partial class Test
{
    [FillMocksWithWrappers] 
    private TestedClass _testedClass = null!;

    [DefaultValue("randomName")]
    private readonly ILoggerFactory _nullFactory = NullLoggerFactory.Instance;
}