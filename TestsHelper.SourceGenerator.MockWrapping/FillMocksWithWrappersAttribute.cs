using System;

namespace TestsHelper.SourceGenerator.MockWrapping;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class FillMocksWithWrappersAttribute : Attribute
{
}