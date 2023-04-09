using TestsHelper.SourceGenerator.MockWrapping;
using Moq;
using Moq.Language.Flow;
using System.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using MyNamespace;

namespace TestsHelper.SourceGenerator.MockWrapping
{
    public class Wrapper_IDependency
    {
        public Mock<IDependency> Mock { get; }

        public Method_MakeString MakeString { get; }

        public Method_Add Add { get; }

        public Wrapper_IDependency(Mock<IDependency> dependencyMock)
        {
            Mock = dependencyMock;
            MakeString = new Method_MakeString(dependencyMock);
            Add = new Method_Add(dependencyMock);
        }

        public class Method_MakeString
        {
            private readonly Expression<Func<IDependency, String>> _expression = _expression => _expression.MakeString(Cyber.Fill<Int32>());
            private readonly Mock<IDependency> _mock;
            public Method_MakeString(Mock<IDependency> mock)
            {
                _mock = mock;
            }

            public ISetup<IDependency, String> Setup(Value<Int32> number = default)
            {
                var expression = Cyber.UpdateExpressionWithParameters(_expression, new[]{Value<Int32>.ConvertValueOrAny(number)});
                return _mock.Setup(expression);
            }

            public void Verify(Value<Int32> number = default, Times? times = null)
            {
                var expression = Cyber.UpdateExpressionWithParameters(_expression, new[]{Value<Int32>.ConvertValueOrAny(number)});
                _mock.Verify(expression, times ?? Times.AtLeastOnce());
            }
        }

        public class Method_Add
        {
            private readonly Expression<Action<IDependency>> _expression = _expression => _expression.Add(Cyber.Fill<String>());
            private readonly Mock<IDependency> _mock;
            public Method_Add(Mock<IDependency> mock)
            {
                _mock = mock;
            }

            public ISetup<IDependency> Setup(Value<String> name = default)
            {
                var expression = Cyber.UpdateExpressionWithParameters(_expression, new[]{Value<String>.ConvertValueOrAny(name)});
                return _mock.Setup(expression);
            }

            public void Verify(Value<String> name = default, Times? times = null)
            {
                var expression = Cyber.UpdateExpressionWithParameters(_expression, new[]{Value<String>.ConvertValueOrAny(name)});
                _mock.Verify(expression, times ?? Times.AtLeastOnce());
            }
        }
    }
}