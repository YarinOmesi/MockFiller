using System;

namespace MockFiller.SourceGenerator.MockWrapping;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class FillMocksWithWrappersAttribute : Attribute
{
}