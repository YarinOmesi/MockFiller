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
        }

        public class Method_Add
        {
            private readonly Expression<Action<IDependency>> _expression = _expression => _expression.Add(Cyber.Fill<String>());
            private readonly Mock<IDependency> _mock;
            public Method_Add(Mock<IDependency> mock)
            {
                _mock = mock;
            }
        }
    }
}