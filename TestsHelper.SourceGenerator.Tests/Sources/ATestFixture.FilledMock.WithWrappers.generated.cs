using Moq;
using TestsHelper.SourceGenerator.MockWrapping.Converters;
using MyNamespace;
using TestsHelper.SourceGenerator.MockWrapping;

namespace MyNamespace
{
    public partial class ATestFixture
    {
        private Wrapper_IDependency _dependency;
        private TestedClass Build()
        {
            var converter = MoqValueConverter.Instance;
            _dependency = new Wrapper_IDependency(new Mock<IDependency>(), converter);
            return new TestedClass(_dependency.Mock.Object, _nullFactory);
        }
    }
}