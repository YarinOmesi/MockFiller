using Microsoft.Extensions.Logging;

namespace MyNamespace;

public class TestedClass
{
    private IDependency _dependency;
    private ILogger _logger;

    public TestedClass(IDependency dependency, ILoggerFactory factory)
    {
        _dependency = dependency;
        _logger = factory.CreateLogger<TestedClass>();
    }
}
