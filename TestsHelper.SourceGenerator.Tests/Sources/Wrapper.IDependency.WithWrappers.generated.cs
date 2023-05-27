using Moq;
using MyNamespace;
using TestsHelper.SourceGenerator.MockWrapping;
using TestsHelper.SourceGenerator.MockWrapping.Converters;
using System.Linq.Expressions;
using System;
using Moq.Language.Flow;

namespace TestsHelper.SourceGenerator.MockWrapping
{
    public class Wrapper_IDependency
    {
        public Mock<IDependency> Mock { get; }

        public Method_MakeString MakeString { get; }

        public Method_Add Add { get; }

        public Wrapper_IDependency(Mock<IDependency> mock, IValueConverter converter)
        {
            Mock = mock;
            MakeString = new Method_MakeString(mock, converter);
            Add = new Method_Add(mock, converter);
        }

        public class Method_MakeString
        {
            private readonly Expression<Func<IDependency, String>> _expression = p => p.MakeString(Cyber.Fill<Int32>());
            private readonly Mock<IDependency> _mock;
            private readonly IValueConverter _converter;
            public Method_MakeString(Mock<IDependency> mock, IValueConverter converter)
            {
                _mock = mock;
                _converter = converter;
            }

            public ISetup<IDependency, String> Setup(Value<Int32> number = default)
            {
                var expression = Cyber.UpdateExpressionWithParameters(_expression, _converter.Convert(number));
                return _mock.Setup(expression);
            }

            public void Verify(Value<Int32> number = default, Times? times = null)
            {
                var expression = Cyber.UpdateExpressionWithParameters(_expression, _converter.Convert(number));
                _mock.Verify(expression, times ?? Times.AtLeastOnce());
            }
        }

        public class Method_Add
        {
            private readonly Expression<Action<IDependency>> _expression = p => p.Add(Cyber.Fill<String>());
            private readonly Mock<IDependency> _mock;
            private readonly IValueConverter _converter;
            public Method_Add(Mock<IDependency> mock, IValueConverter converter)
            {
                _mock = mock;
                _converter = converter;
            }

            public ISetup<IDependency> Setup(Value<String> name = default)
            {
                var expression = Cyber.UpdateExpressionWithParameters(_expression, _converter.Convert(name));
                return _mock.Setup(expression);
            }

            public void Verify(Value<String> name = default, Times? times = null)
            {
                var expression = Cyber.UpdateExpressionWithParameters(_expression, _converter.Convert(name));
                _mock.Verify(expression, times ?? Times.AtLeastOnce());
            }
        }
    }
}