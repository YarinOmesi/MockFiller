using Moq;
using MyNamespace;

namespace TestsHelper.SourceGenerator.MockWrapping
{
    public class Wrapper_IDependency
    {
        public Mock<IDependency> Mock { get; }

        public Wrapper_IDependency(Mock<IDependency> mock)
        {
            Mock = mock;
        }
    }
}