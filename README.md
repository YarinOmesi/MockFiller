# Mock Filler
[![Build](https://github.com/YarinOmesi/MockFiller/actions/workflows/CI.yml/badge.svg)](https://github.com/YarinOmesi/MockFiller/actions/workflows/CI.yml)

Creating tested class instance with mocks!

Refer to [Test File Example](./Sample.Tests/Test.cs) to see an example,
Or the tests [Source Generator Tests](./TestsHelper.SourceGenerator.Tests/MockFillerSourceGeneratorTests.cs).

## How To Use

All you need to do is to mark your test fixture class as `partial`.

Create field of the desired tested class and mark it with attribute `[FillMocks]`.

The Source Generator will Create **a field for each mocked parameters with the same name**, and create a `Build()` method to create the instance. 

### Features

#### FillMocks

To Fill Mocks for given tested class, create a field for that class and mark it with `[FillMocks]`
```csharp
[FillMocks]
private TestedClass _testedClass;
```

To access a mock of parameter named `loggerFactory`
```csharp
_loggerFactory.Mock // Mock<ILoggerFactory>
```

#### Default Value

To declare a default value instead of creating a mock, create a field with name as following:

defaultValue[Constructor Parameter Name]

> :alert: Be aware that default value is case insensitive

To set a default value for parameter named `factory`
```csharp
private ILoggerFactory _defaultValueFactory = NullLoggerFactory.Instance;
```

#### Generate Mock Wrappers

Generate Mock Wrappers By marking the class with `[TestsHelper.SourceGenerator.MockWrapping.GenerateMockWrappers]` attribute.

The mocked parameter **field will have a property for each public method** in the mocked type. 

A `Setup` and `Verify` methods will be generated for each public method of the mocked type.

##### Demonstration

instead of doing this

```csharp
// Setup
_dependencyMock.Setup(dependency => dependency.MakeString(It.IsAny<int>(), "Yarin"))
    .Returns<int>((number) => number.ToString());

// Verify
_dependencyMock.Verify(dependency => dependency.MakeString(It.IsAny<int>(), "Yarin"), Times.Once)
```

you can do this

```csharp
/* -- Setup -- */ 
// Default parameter is Any
_dependency.MakeString.Setup(name:"Yarin")
    .Returns<int>(n=> n.ToString());

// Any Not Implicitly assumed
_dependency.MakeString.Setup(Value<int>.Any, "Yarin")
    .Returns<int>(n=> n.ToString());

/* -- Verify -- */
_dependency.MakeString.Verify(Value<int>.Any, "Yarin", Times.Once())
```

### Example

For This Code

```csharp
// Class Being Tested
public class TestedClass
{
    private IDependency _dependency;
    private ILogger _logger;

    public TestedClass(IDependency dependency, ILoggerFactory factory)
    {
        /* Code */    
    }
}

// Test Fixture class
public partial class Test
{
    [FillMocks]
    private TestedClass _testedClass;

    /* Rest Of Implementation... */
}
```

The Generated Code Will Be

```csharp
public partial class Test
    {
        private Wrapper_IDependency _dependency;
        private Wrapper_ILoggerFactory _factory;

        private TestedClass Build()
        {
            _dependency = new Wrapper_IDependency(new Mock<IDependency>());
            _factory = new Wrapper_ILoggerFactory(new Mock<ILoggerFactory>());
            return new TestedClass(_dependency.Mock.Object, _factory.Mock.Object);
        }
    }
```
Example of Wrapper class with `[GenerateMockWrappers]` attribute [IDependency With Wrappers](./TestsHelper.SourceGenerator.Tests/Sources/Wrapper.IDependency.WithWrappers.generated.cs)

Example of Wrapper class without mocked wrappers  [IDependency Without Wrappers](./TestsHelper.SourceGenerator.Tests/Sources/Wrapper.IDependency.generated.cs)