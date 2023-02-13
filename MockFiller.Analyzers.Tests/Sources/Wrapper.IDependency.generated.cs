using MockFiller.MockWrapping;
using Moq;
using Moq.Language.Flow;
using System.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using MyNamespace;

namespace MockFiller.MockWrapping
{
    public class Wrapper_IDependency
    {
        public Mock<IDependency> Mock { get; }

        public Wrapper_IDependency(Mock<IDependency> dependencyMock)
        {
            Mock = dependencyMock;
        }
    }
}