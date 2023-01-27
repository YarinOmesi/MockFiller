using TestsHelper.SourceGenerator.MockWrapping;
using Moq;

namespace MyNamespace
{
    public partial class ATestFixture
    {
        private Wrapper_IDependency _dependency;
        private TestedClass Build()
        {
            _dependency = new Wrapper_IDependency(new Mock<IDependency>());
            return new TestedClass(_dependency.Mock.Object, _defaultValueFactory);
        }
    }
}