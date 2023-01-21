using System;

namespace TestsHelper.SourceGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class FillMocksAttribute : Attribute
    {
    }
}