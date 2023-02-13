﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using TestsHelper.SourceGenerator.Attributes;
using TestsHelper.SourceGenerator.MockWrapping;

namespace Sample.Tests;

[TestFixture]
public partial class Test
{
    [FillMocksWithWrappers] 
    private TestedClass _testedClass = null!;

    [DefaultValue("factory")]
    private readonly ILoggerFactory _nullFactory = NullLoggerFactory.Instance;

    [SetUp]
    public void Setup()
    {
        _testedClass = Build();
    }

    [Test]
    public void Setup_WithoutWrapper()
    {
        // Arrange
        int number = 1;

        _dependency.Mock.Setup(dependency => dependency.MakeString(It.IsAny<int>()))
            .Returns<int>(n => n.ToString());

        // Act
        string result = _testedClass.VeryComplicatedLogic(number);

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