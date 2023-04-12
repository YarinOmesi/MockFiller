using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Moq;
using TestsHelper.SourceGenerator.MockWrapping.Values;

namespace TestsHelper.SourceGenerator.MockWrapping.Converters;

public class MoqValueConverter : IValueConverter
{
    public static readonly MoqValueConverter Instance = new(new Dictionary<Type, IValueConverter> {
        [typeof(AnyValue<>)] = new AnyValueConverter(),
        [typeof(ExactValue<>)] = new ExactValueConverter(),
        [typeof(PredicateValue<>)] = new PredicateValueConverter()
    });

    private readonly Dictionary<Type, IValueConverter> _valueConverters;

    private MoqValueConverter(Dictionary<Type, IValueConverter> valueConverters)
    {
        _valueConverters = valueConverters;
    }

    public Expression Convert<T>(Value<T> value)
    {
        value ??= Value<T>.Any;
        
        Type typeWithoutGenericArgument = value.GetType().GetGenericTypeDefinition();
        if (_valueConverters.TryGetValue(typeWithoutGenericArgument, out IValueConverter converter))
        {
            return converter.Convert(value);
        }

        throw new ArgumentException($"Could not convert value of type {value.GetType()}");
    }

    private class AnyValueConverter : IValueConverter
    {
        public Expression Convert<T>(Value<T> value)
        {
            Expression<Func<T>> isAnyExpression = () => It.IsAny<T>();
            return isAnyExpression.Body;
        }
    }
    
    private class ExactValueConverter : IValueConverter
    {
        public Expression Convert<T>(Value<T> value)
        {
            ExactValue<T> exactValue = (ExactValue<T>) value;
            Expression<Func<T>> itIsExpression = () => It.Is<T>(Cyber.FillValue<T>(), EqualityComparer<T>.Default);
            
            return ExpressionUtils.GetBodyWithUpdatedFirstArgument(itIsExpression, Expression.Constant(exactValue.Value));
        }
    }
    
    private class PredicateValueConverter : IValueConverter
    {
        public Expression Convert<T>(Value<T> value)
        {
            PredicateValue<T> predicateValue = (PredicateValue<T>) value;
            Expression<Func<T>> itIsExpression = () => It.Is<T>(Cyber.FillPredicate<T>());
            
            return ExpressionUtils.GetBodyWithUpdatedFirstArgument(itIsExpression, predicateValue.Predicate);
        }
    }
}