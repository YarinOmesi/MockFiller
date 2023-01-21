using Moq;

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
    }
}