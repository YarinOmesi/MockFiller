using Moq;
using MyNamespace;
using TestsHelper.SourceGenerator.MockWrapping;

namespace MyNamespace
{
    public partial class ATestFixture
    {
        private Wrapper_IDependency _dependency;
        private TestedClass Build()
        {
            _dependency = new Wrapper_IDependency(new Mock<IDependency>());
            return new TestedClass(_dependency.Mock.Object, _nullFactory);
        }
    }
}