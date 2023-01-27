using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using TestsHelper.SourceGenerator.Attributes;
using TestsHelper.SourceGenerator.MockWrapping;

namespace MyNamespace;

[GenerateMockWrappers]
public partial class ATestFixture
{
    [FillMocks]
    private TestedClass _testedClass;
    private readonly ILoggerFactory _defaultValueFactory = NullLoggerFactory.Instance;
}