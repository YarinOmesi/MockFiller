using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using TestsHelper.SourceGenerator.Attributes;

namespace Sample.Tests;

[TestFixture]
public partial class Test
{
    [FillMocks]
    private TestedClass _testedClass;

    
    //private readonly ILoggerFactory _defaultValueFactory = NullLoggerFactory.Instance;

    [SetUp]
    public void Setup()
    {
        _testedClass = Build();
    }

    [Test]
    public void METHOD_On_Effect()
    {
        // Arrange
        
        // Act
        _testedClass.VeryComplicatedLogic();
        // Assert

    }
}

