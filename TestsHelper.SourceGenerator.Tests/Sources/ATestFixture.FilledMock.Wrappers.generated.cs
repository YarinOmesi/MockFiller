using TestsHelper.SourceGenerator.MockWrapping;
using Moq;
using Moq.Language.Flow;
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

        private void Verify_dependency_MakeString(Value<Int32>? number = null, Times? times = null)
        {
            Expression<Func<IDependency, String>> expression = dependency => dependency.MakeString(Cyber.Fill<Int32>());
            expression = Cyber.UpdateExpressionWithParameters(expression, new[]{Cyber.CreateExpressionFor(number ?? Value<Int32>.Any)});
            _dependencyMock.Verify(expression, times ?? Times.AtLeastOnce());
        }

        private ISetup<IDependency> Setup_dependency_Add(Value<String>? name = null)
        {
            Expression<Action<IDependency>> expression = dependency => dependency.Add(Cyber.Fill<String>());
            expression = Cyber.UpdateExpressionWithParameters(expression, new[]{Cyber.CreateExpressionFor(name ?? Value<String>.Any)});
            return _dependencyMock.Setup(expression);
        }

        private void Verify_dependency_Add(Value<String>? name = null, Times? times = null)
        {
            Expression<Action<IDependency>> expression = dependency => dependency.Add(Cyber.Fill<String>());
            expression = Cyber.UpdateExpressionWithParameters(expression, new[]{Cyber.CreateExpressionFor(name ?? Value<String>.Any)});
            _dependencyMock.Verify(expression, times ?? Times.AtLeastOnce());
        }
    }
}