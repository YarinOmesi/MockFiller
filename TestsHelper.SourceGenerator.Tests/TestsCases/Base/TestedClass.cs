using Microsoft.Extensions.Logging;

namespace TestsHelper.SourceGenerator.Tests.TestsCases.Base;

public class TestedClass
{
    private IDependency _dependency;
    private ILogger _logger;

    public TestedClass(IDependency dependency, ILoggerFactory factory)
    {
        _dependency = dependency;
        _logger = factory.CreateLogger<TestedClass>();
    }

    public string VeryComplicatedLogic(int number)
    {
        return _dependency.MakeString(number + 1);
    }
}