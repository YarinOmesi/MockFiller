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

            public ISetup<IDependency, String> Setup(Value<Int32>? number = null)
            {
                var expression = Cyber.UpdateExpressionWithParameters(_expression, new[]{Cyber.CreateExpressionFor(number ?? Value<Int32>.Any)});
                return _mock.Setup(expression);
            }

            public void Verify(Value<Int32>? number = null, Times? times = null)
            {
                var expression = Cyber.UpdateExpressionWithParameters(_expression, new[]{Cyber.CreateExpressionFor(number ?? Value<Int32>.Any)});
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

            public ISetup<IDependency> Setup(Value<String>? name = null)
            {
                var expression = Cyber.UpdateExpressionWithParameters(_expression, new[]{Cyber.CreateExpressionFor(name ?? Value<String>.Any)});
                return _mock.Setup(expression);
            }

            public void Verify(Value<String>? name = null, Times? times = null)
            {
                var expression = Cyber.UpdateExpressionWithParameters(_expression, new[]{Cyber.CreateExpressionFor(name ?? Value<String>.Any)});
                _mock.Verify(expression, times ?? Times.AtLeastOnce());
            }
        }
    }
}