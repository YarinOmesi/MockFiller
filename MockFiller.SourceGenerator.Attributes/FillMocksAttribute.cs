using System;

namespace MockFiller.SourceGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class FillMocksAttribute : Attribute
    {
    }
}