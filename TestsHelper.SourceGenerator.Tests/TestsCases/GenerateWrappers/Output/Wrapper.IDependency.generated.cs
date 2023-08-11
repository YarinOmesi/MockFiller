using GeneratedCodeAttribute = global::System.CodeDom.Compiler.GeneratedCodeAttribute;
using Mock_IDependency = global::Moq.Mock<global::TestsHelper.SourceGenerator.Tests.TestsCases.Base.IDependency>;
using Wrapper_IDependency_Method_MakeString = global::TestsHelper.SourceGenerator.Tests.TestsCases.GenerateWrappers.Source.Wrapper_IDependency.Method_MakeString;
using Wrapper_IDependency_Method_Add = global::TestsHelper.SourceGenerator.Tests.TestsCases.GenerateWrappers.Source.Wrapper_IDependency.Method_Add;
using IValueConverter = global::TestsHelper.SourceGenerator.MockWrapping.Converters.IValueConverter;
using Expression_Func_IDependency_String = global::System.Linq.Expressions.Expression<global::System.Func<global::TestsHelper.SourceGenerator.Tests.TestsCases.Base.IDependency, global::System.String>>;
using Cyber = global::TestsHelper.SourceGenerator.MockWrapping.Cyber;
using Int32 = global::System.Int32;
using ISetup_IDependency_String = global::Moq.Language.Flow.ISetup<global::TestsHelper.SourceGenerator.Tests.TestsCases.Base.IDependency, global::System.String>;
using Value_Int32 = global::TestsHelper.SourceGenerator.MockWrapping.Value<global::System.Int32>;
using Times = global::Moq.Times;
using Expression_Action_IDependency = global::System.Linq.Expressions.Expression<global::System.Action<global::TestsHelper.SourceGenerator.Tests.TestsCases.Base.IDependency>>;
using String = global::System.String;
using ISetup_IDependency = global::Moq.Language.Flow.ISetup<global::TestsHelper.SourceGenerator.Tests.TestsCases.Base.IDependency>;
using Value_String = global::TestsHelper.SourceGenerator.MockWrapping.Value<global::System.String>;

// <auto-generated/>
namespace TestsHelper.SourceGenerator.Tests.TestsCases.GenerateWrappers.Source
{
    [GeneratedCodeAttribute("TestsHelper.SourceGenerator", "3.0.0.0")]
    public class Wrapper_IDependency
    {
        public Mock_IDependency Mock { get; }

        public Wrapper_IDependency_Method_MakeString MakeString { get; }

        public Wrapper_IDependency_Method_Add Add { get; }

        public Wrapper_IDependency(Mock_IDependency mock, IValueConverter converter)
        {
            Mock = mock;
            MakeString = new Wrapper_IDependency_Method_MakeString(mock, converter);
            Add = new Wrapper_IDependency_Method_Add(mock, converter);
        }

        [GeneratedCodeAttribute("TestsHelper.SourceGenerator", "3.0.0.0")]
        public class Method_MakeString
        {
            private readonly Expression_Func_IDependency_String _expression = p => p.MakeString(Cyber.Fill<Int32>());
            private readonly Mock_IDependency _mock;
            private readonly IValueConverter _converter;
            public Method_MakeString(Mock_IDependency mock, IValueConverter converter)
            {
                _mock = mock;
                _converter = converter;
            }

            public ISetup_IDependency_String Setup(Value_Int32 number = default)
            {
                var expression = Cyber.UpdateExpressionWithParameters(_expression, _converter.Convert(number));
                return _mock.Setup(expression);
            }

            public void Verify(Value_Int32 number = default, Times? times = null)
            {
                var expression = Cyber.UpdateExpressionWithParameters(_expression, _converter.Convert(number));
                _mock.Verify(expression, times ?? Times.AtLeastOnce());
            }
        }

        [GeneratedCodeAttribute("TestsHelper.SourceGenerator", "3.0.0.0")]
        public class Method_Add
        {
            private readonly Expression_Action_IDependency _expression = p => p.Add(Cyber.Fill<String>());
            private readonly Mock_IDependency _mock;
            private readonly IValueConverter _converter;
            public Method_Add(Mock_IDependency mock, IValueConverter converter)
            {
                _mock = mock;
                _converter = converter;
            }

            public ISetup_IDependency Setup(Value_String name = default)
            {
                var expression = Cyber.UpdateExpressionWithParameters(_expression, _converter.Convert(name));
                return _mock.Setup(expression);
            }

            public void Verify(Value_String name = default, Times? times = null)
            {
                var expression = Cyber.UpdateExpressionWithParameters(_expression, _converter.Convert(name));
                _mock.Verify(expression, times ?? Times.AtLeastOnce());
            }
        }
    }
}