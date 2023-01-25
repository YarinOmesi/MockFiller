using Moq.Language.Flow;
using Moq;
using System.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyNamespace
{
    public partial class ATestFixture
    {
        private Mock<IDependency> _dependencyMock;
        private TestedClass Build()
        {
            _dependencyMock = new Mock<IDependency>();
            return new TestedClass(_dependencyMock.Object, _defaultValueFactory);
        }

        private ISetup<IDependency, String> Setup_dependency_MakeString(Value<Int32>? number = null)
        {
            Expression<Func<IDependency, String>> expression = dependency => dependency.MakeString(Cyber.Fill<Int32>());
            expression = Cyber.UpdateExpressionWithParameters(expression, new[]{Cyber.CreateExpressionFor(number ?? Value<Int32>.Any)});
            return _dependencyMock.Setup(expression);
        }

        private ISetup<IDependency> Setup_dependency_Add(Value<String>? name = null)
        {
            Expression<Action<IDependency>> expression = dependency => dependency.Add(Cyber.Fill<String>());
            expression = Cyber.UpdateExpressionWithParameters(expression, new[]{Cyber.CreateExpressionFor(name ?? Value<String>.Any)});
            return _dependencyMock.Setup(expression);
        }

        public static class Cyber
        {
            // This Method Should Not Be Ran It Used As A Filler For Types
            public static T Fill<T>() => throw new NotImplementedException();
            public static Expression<T> UpdateExpressionWithParameters<T>(Expression<T> expression, IEnumerable<Expression> arguments)
            {
                MethodCallExpression body = (MethodCallExpression)expression.Body;
                return expression.Update(body.Update(body.Object, arguments), expression.Parameters);
            }

            public static Expression CreateExpressionFor<T>(Value<T> value)
            {
                Expression<Func<T>> isAny = () => It.IsAny<T>();
                Expression<Func<T>> itIs = () => It.Is<T>(Fill<T>(), EqualityComparer<T>.Default);
                MethodCallExpression body = (MethodCallExpression)itIs.Body;
                if (value.IsAny)
                {
                    return isAny.Body;
                }

                List<Expression> newArguments = body.Arguments.ToList();
                newArguments[0] = Expression.Constant(value.IsDefault ? default : value._Value);
                return body.Update(body.Object, newArguments);
            }
        }

        public readonly struct Value<T>
        {
            public static readonly Value<T> Any = new(default, isAny: true);
            public static readonly Value<T> Default = new(default, isDefault: true);
            public T _Value { get; }

            public bool IsAny { get; }

            public bool IsDefault { get; }

            public Value(T value, bool isDefault = false, bool isAny = false)
            {
                _Value = value;
                IsDefault = isDefault;
                IsAny = isAny;
            }

            public static implicit operator Value<T>(T value) => new(value);
        }
    }
}