using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using MockFiller.Attributes;

namespace MyNamespace;

public partial class ATestFixture
{
    [FillMocks]
    private TestedClass _testedClass;
    
    [DefaultValue("NonExistingParameterName")]
    private readonly ILoggerFactory _nullFactory = NullLoggerFactory.Instance;
}