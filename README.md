# Mock Filler
[![Build](https://github.com/YarinOmesi/MockFiller/actions/workflows/CI.yml/badge.svg)](https://github.com/YarinOmesi/MockFiller/actions/workflows/CI.yml)

Creating tested class instance with mocks!

refer to [Test File Example](./Sample.Tests/Test.cs) to see an example
and [Source Generator Tests](./TestsHelper.SourceGenerator.Tests/MockFillerSourceGeneratorTests.cs).

## How To Use

All you need to do is to mark your test fixture class as `partial`.

Create field of the desired tested class and mark it with attribute `[FillMocks]`.

The Source Generator will fill mocks field and create a `Build()` method to create the instance.

### Features

#### FillMocks

To Fill Mocks for given tested class, create a field for that class and mark it with `[FillMocks]`

#### Default Value

To declare a default value instead of creating a mock, create a field with name as following:

defaultValue[Constructor Parameter Name]

> you can add underscore and change the casing

##### Demonstration

```csharp
private ILoggerFactory _defaultValueFactory = NullLoggerFactory.Instance;
```

#### Generate Mock Wrappers

By marking the test fixture cass with `[TestsHelper.SourceGenerator.MockWrapping.GenerateMockWrappers]` attribute,
it will generate mock wrappers.

A Setup and verify method will be generated for each public method of dependencies.

Setup method name template `Setup_<ParameterName>_<MethodName>()`
Verify method name template `Verify_<ParameterName>_<MethodName>()`

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
Setup_dependency_MakeString(name:"Yarin")
    .Returns<int>(n=> n.ToString());

// Any Not Implicitly assumed
Setup_dependency_MakeString(Value<int>.Any,"Yarin")
    .Returns<int>(n=> n.ToString());

/* -- Verify -- */
Setup_dependency_MakeString(Value<int>.Any,"Yarin", Times.Once())
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
    private Mock<IDependency> _dependencyMock;
    private Mock<ILoggerFactory> _factoryMock;
    
    private TestedClass Build()
    {
        _dependencyMock = new Mock<IDependency>();
        _factoryMock = new Mock<ILoggerFactory>();
        return new TestedClass(_dependencyMock.Object, _factoryMock.Object);
    }
}
```
