using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using TestsHelper.SourceGenerator.Attributes;
using TestsHelper.SourceGenerator.MockWrapping;

namespace Sample.Tests;

[GenerateMockWrappers]
[TestFixture]
public partial class Test
{
    [FillMocks]
    private TestedClass _testedClass;

    
    private readonly ILoggerFactory _defaultValueFactory = NullLoggerFactory.Instance;

    [SetUp]
    public void Setup()
    {
        _testedClass = Build();
    }

    [Test]
    public void Setup_WithoutWrapper()
    {
        // Arrange
        int numbrer = 1;


        _dependencyMock.Setup(dependency => dependency.MakeString(It.IsAny<int>()))
            .Returns<int>((number) => number.ToString());

        // Act
        string result = _testedClass.VeryComplicatedLogic(numbrer);

        // Assert
        Assert.That(result, Is.EqualTo("2"));
    }

    [Test]
    public void Setup_WithWrapper()
    {
        // Arrange
        int number = 1;


        Setup_dependency_MakeString()
            .Returns<int>(n=> n.ToString());

        // Act
        string result = _testedClass.VeryComplicatedLogic(number);

        // Assert
        Assert.That(result, Is.EqualTo("2"));
    }
}