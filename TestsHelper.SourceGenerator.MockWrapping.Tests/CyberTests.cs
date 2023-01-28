using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using TestsHelper.SourceGenerator.MockWrapping.Tests.Expressions;

namespace TestsHelper.SourceGenerator.MockWrapping.Tests;

[TestFixture]
public class CyberTests
{
    private ExpressionComparison _expressionComparison = null!;

    [OneTimeSetUp]
    public void Setup()
    {
        _expressionComparison = new ExpressionComparison();
    }

    [Test]
    public void CreateExpressionFor_GivenAnyValue_CallItAny()
    {
        // Arrange
        Value<int> anyValue = Value<int>.Any;
        var expectedMethodCallExpression = GetLambdaBody<MethodCallExpression>(() => It.IsAny<int>());

        // Act
        Expression actualExpression = Cyber.CreateExpressionFor(anyValue);

        // Assert
        _expressionComparison.AssertEquals(actualExpression, expectedMethodCallExpression);
    }

    [Test]
    public void CreateExpressionFor_GivenValue_CallItIs()
    {
        // Arrange
        Value<int> value = 5;
        var expectedMethodCallExpression = GetLambdaBody<MethodCallExpression>(() => It.Is(5, EqualityComparer<int>.Default));

        // Act
        Expression actualExpression = Cyber.CreateExpressionFor(value);

        // Assert
        _expressionComparison.AssertEquals(actualExpression, expectedMethodCallExpression);
    }

    [Test]
    public void UpdateExpressionWithParameters_OnAllParameterNull_PatchExpressionToUseItAny()
    {
        // Arrange
        var arguments = new List<Expression> {
            Cyber.CreateExpressionFor(Value<int>.Any),
            Cyber.CreateExpressionFor(Value<string>.Any),
        };
        
        Expression<Action> method = () => MockMethod(Cyber.Fill<int>(), Cyber.Fill<string>());
        Expression<Action> expectedExpression = () => MockMethod(It.IsAny<int>(), It.IsAny<string>());
        
        // Act
        Expression<Action> patchedExpression = Cyber.UpdateExpressionWithParameters(method, arguments);

        // Assert
        _expressionComparison.AssertEquals(patchedExpression, expectedExpression);
    } 
    
    [Test]
    public void UpdateExpressionWithParameters_OnSomeAnyAndValue_PatchExpressionToUseItAnyOrItIs()
    {
        // Arrange
        var arguments = new List<Expression> {
            Cyber.CreateExpressionFor<int>(10),
            Cyber.CreateExpressionFor(Value<string>.Any),
        };
        
        Expression<Action> method = () => MockMethod(Cyber.Fill<int>(), Cyber.Fill<string>());
        Expression<Action> expectedExpression = () => MockMethod(It.Is(10, EqualityComparer<int>.Default), It.IsAny<string>());
        
        // Act
        Expression<Action> patchedExpression = Cyber.UpdateExpressionWithParameters(method, arguments);

        // Assert
        _expressionComparison.AssertEquals(patchedExpression, expectedExpression);
    }

    private void MockMethod(int age, string name)
    {
        
    }

    private static T GetLambdaBody<T>(Expression<Action> expression) where T : Expression => (T) expression.Body;
}