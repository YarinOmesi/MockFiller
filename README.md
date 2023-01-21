# Mock Filler

Creating tested class instance with mocks!

refer to [Test File Example](./Sample.Tests/Test.cs) to see an example

## How To Use

All you need to do is to mark you test fixture class as `partial`.

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

### Example

For This Code

```csharp
// Class Being Tested
public class TestedClass
{
    private ILogger _logger;
    
    public TestedClass(ILoggerFactory factory)
    {
    
    }
}

// Test Fixture class
public partial class MyTestFixture
{
    [FillMocks]
    private TestedClass _testedClasss;
    
   /* Rest Of The Class */
}
```

The Generated Code Will Be

```csharp
public partial class MyTestFixture
{
    private Mock<ILoggerFactory> _loggerFactoryMock;
    
   private TestedClass Build()
   {
        return new TestedClass(_loggerFactoryMock.Object);
   }
}
```