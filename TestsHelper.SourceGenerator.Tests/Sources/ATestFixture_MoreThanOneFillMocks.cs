using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using TestsHelper.SourceGenerator.Attributes;

namespace MyNamespace;

public partial class ATestFixture
{
    [FillMocks]
    private TestedClass _testedClass;
    
    [FillMocks]
    private TestedClass _testedClass2;

    [DefaultValue("factory")]
    private readonly ILoggerFactory _nullFactory = NullLoggerFactory.Instance;
}