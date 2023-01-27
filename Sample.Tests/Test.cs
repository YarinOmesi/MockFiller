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


        
        _dependency.Mock.Setup(dependency => dependency.MakeString(It.IsAny<int>()))
            .Returns<int>((number) => number.ToString());

        // Act
        string result = _testedClass.VeryComplicatedLogic(numbrer);

        // Assert
        _dependency.Mock.Verify(dependency => dependency.MakeString(2), Times.Once);
        Assert.That(result, Is.EqualTo("2"));
    }

    [Test]
    public void Setup_WithWrapper()
    {
        // Arrange
        int number = 1;


        _dependency.MakeString.Setup()
            .Returns<int>(n => n.ToString());

        // Act
        string result = _testedClass.VeryComplicatedLogic(number);

        // Assert
        _dependency.MakeString.Verify(2, Times.Once());
        Assert.That(result, Is.EqualTo("2"));
    }
}