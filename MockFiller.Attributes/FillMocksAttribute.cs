using System;

namespace MockFiller.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class FillMocksAttribute : Attribute
    {
    }
}