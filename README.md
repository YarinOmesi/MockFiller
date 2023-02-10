# Mock Filler :star:
[![Build](https://github.com/YarinOmesi/MockFiller/actions/workflows/CI.yml/badge.svg)](https://github.com/YarinOmesi/MockFiller/actions/workflows/CI.yml)

Creating tested class instance with mocks!

Refer to [Test File Example](./Sample.Tests/Test.cs) to see an example,
Or the tests [Source Generator Tests](./TestsHelper.SourceGenerator.Tests/MockFillerSourceGeneratorTests.cs).

## How To Use

All you need to do is to mark your test fixture class as `partial`.

Create field of the desired tested class and mark it with attribute `[FillMocks]`.

The Source Generator will Create **a field for each mocked parameters with the same name**, and create a `Build()` method to create the instance. 

## Features

### FillMocks

To Fill Mocks for given tested class, create a field for that class and mark it with `[FillMocks]`
```csharp
[FillMocks]
private TestedClass _testedClass;
```

To access a mock of parameter named `loggerFactory`
```csharp
_loggerFactory.Mock // Mock<ILoggerFactory>
```

### Default Value

To declare a default value instead of creating a mock, mark a field with `[DefaultValue("FieldName")]` attribute.

Example For setting a default value for parameter named `factory`:
```csharp
[DefaultValue("factory")]
private ILoggerFactory _nullLoggerFactory = NullLoggerFactory.Instance;
```

### Generate Mock Wrappers :crystal_ball:

Generate mock wrappers by marking the test class filed with `[FillMocksWithWrappers]` attribute instead of `[FillMocks]`.

The mocked parameter **field will have a property for each public method** in the mocked type. 

A `Setup` and `Verify` methods will be generated for each public method of the mocked type.

#### Demonstration

##### Using Setup
original
```csharp
_dependencyMock.Setup(dependency => dependency.MakeString(It.IsAny<int>()))
    .Returns<int>((number) => number.ToString());
```
With MockFiller
```csharp
_dependency.MakeString.Setup()
    .Returns<int>(n=> n.ToString());
```

##### Using Verify
original
```csharp
_dependencyMock.Verify(dependency => dependency.MakeString(It.IsAny<int>()), Times.Once)
```
With MockFiller
```csharp
_dependency.MakeString.Verify(Times.Once())
```

## Examples
Examples Of Mocked Type [IDependency](./TestsHelper.SourceGenerator.Tests/Sources/IDependency.cs)
When Generating Wrappers And When Don't.

| Attribute                 | Example Link                                                                                                           |
|---------------------------|------------------------------------------------------------------------------------------------------------------------|
| `[FillMocks]`             | [IDependency without wrappers](./TestsHelper.SourceGenerator.Tests/Sources/Wrapper.IDependency.generated.cs)           |
| `[FillMocksWithWrappers]` | [IDependency with wrappers](./TestsHelper.SourceGenerator.Tests/Sources/Wrapper.IDependency.WithWrappers.generated.cs) |