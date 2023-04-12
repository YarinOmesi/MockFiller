using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using TestsHelper.SourceGenerator.MockWrapping.Converters;
using TestsHelper.SourceGenerator.MockWrapping.Tests.Expressions;

namespace TestsHelper.SourceGenerator.MockWrapping.Tests;

[TestFixture]
public class MoqValueConverterTests
{
    private MoqValueConverter _moqValueConverter = null!;
    private ExpressionComparison _expressionComparison = null!;

    [SetUp]
    public void Setup()
    {
        _moqValueConverter = MoqValueConverter.Instance;
        _expressionComparison = new ExpressionComparison();
    }

    [Test]
    public void Convert_GivenAnyValue_CallItAny()
    {
        // Arrange
        Value<int> anyValue = Value<int>.Any;
        var expectedMethodCallExpression = GetLambdaBody<MethodCallExpression>(() => It.IsAny<int>());

        // Act
        Expression actualExpression = _moqValueConverter.Convert(anyValue);

        // Assert
        _expressionComparison.AssertEquals(actualExpression, expectedMethodCallExpression);
    }

    [Test]
    public void Convert_GivenValue_CallItIs()
    {
        // Arrange
        Value<int> value = 5;
        var expectedMethodCallExpression = GetLambdaBody<MethodCallExpression>(() => It.Is(5, EqualityComparer<int>.Default));

        // Act
        Expression actualExpression = _moqValueConverter.Convert(value);

        // Assert
        _expressionComparison.AssertEquals(actualExpression, expectedMethodCallExpression);
    }

    [Test]
    public void Convert_GivenPredicate_CallItIs()
    {
        // Arrange
        Value<int> value = Value<int>.Is(i => i == 10);
        var expectedMethodCallExpression = GetLambdaBody<MethodCallExpression>(() => It.Is<int>(i => i == 10));

        // Act
        Expression actualExpression = _moqValueConverter.Convert(value);

        // Assert
        _expressionComparison.AssertEquals(actualExpression, expectedMethodCallExpression);
    }

    private static T GetLambdaBody<T>(Expression<Action> expression) where T : Expression => (T) expression.Body;
}