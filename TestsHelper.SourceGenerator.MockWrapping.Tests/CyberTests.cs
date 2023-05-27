using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using TestsHelper.SourceGenerator.MockWrapping.Converters;
using TestsHelper.SourceGenerator.MockWrapping.Tests.Expressions;

namespace TestsHelper.SourceGenerator.MockWrapping.Tests;

[TestFixture]
public class CyberTests
{
    private IValueConverter _valueConverter = null!;
    private ExpressionComparison _expressionComparison = null!;

    [OneTimeSetUp]
    public void Setup()
    {
        _valueConverter = MoqValueConverter.Instance;
        _expressionComparison = new ExpressionComparison();
    }

    [Test]
    public void UpdateExpressionWithParameters_OnAllParameterNull_PatchExpressionToUseItAny()
    {
        // Arrange
        var arguments = new List<Expression> {
            _valueConverter.Convert(Value<int>.Any),
            _valueConverter.Convert(Value<string>.Any),
        };

        Expression<Action> method = () => MockMethod(Cyber.Fill<int>(), Cyber.Fill<string>());
        Expression<Action> expectedExpression = () => MockMethod(It.IsAny<int>(), It.IsAny<string>());

        // Act
        Expression<Action> patchedExpression = Cyber.UpdateExpressionWithParameters(method, arguments.ToArray());

        // Assert
        _expressionComparison.AssertEquals(patchedExpression, expectedExpression);
    }

    [Test]
    public void UpdateExpressionWithParameters_OnSomeAnyAndValue_PatchExpressionToUseItAnyOrItIs()
    {
        // Arrange
        var arguments = new List<Expression> {
            _valueConverter.Convert(Value<int>.Is(10)),
            _valueConverter.Convert(Value<string>.Any),
        };

        Expression<Action> method = () => MockMethod(Cyber.Fill<int>(), Cyber.Fill<string>());
        Expression<Action> expectedExpression = () => MockMethod(It.Is(10, EqualityComparer<int>.Default), It.IsAny<string>());

        // Act
        Expression<Action> patchedExpression = Cyber.UpdateExpressionWithParameters(method, arguments.ToArray());

        // Assert
        _expressionComparison.AssertEquals(patchedExpression, expectedExpression);
    }

    private void MockMethod(int age, string name)
    {
    }
}