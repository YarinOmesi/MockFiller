using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using MockFillerMockFiller.Attributes;

namespace MyNamespace;

public partial class ATestFixture
{
    [FillMocks]
    private TestedClass _testedClass;
    
    [DefaultValue("factory")]
    private readonly int _nullFactory = 6;
}