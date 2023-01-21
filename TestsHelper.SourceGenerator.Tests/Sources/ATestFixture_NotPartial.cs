using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using TestsHelper.SourceGenerator.Attributes;

namespace MyNamespace;

public class ATestFixture
{
    [FillMocks]
    private TestedClass _testedClass;
    private readonly ILoggerFactory _defaultValueFactory = NullLoggerFactory.Instance;
}