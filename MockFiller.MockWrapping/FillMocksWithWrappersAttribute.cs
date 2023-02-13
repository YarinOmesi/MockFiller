using System;

namespace MockFiller.MockWrapping;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class FillMocksWithWrappersAttribute : Attribute
{
}