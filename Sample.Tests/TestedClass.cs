using Microsoft.Extensions.Logging;

namespace Sample.Tests;

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

    public Response ExecuteRequest(Request request) => _dependency.ExecuteRequest(request);
}