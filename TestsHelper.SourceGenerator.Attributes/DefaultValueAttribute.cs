using System;

namespace TestsHelper.SourceGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class DefaultValueAttribute : Attribute
    {
        public string Field { get; }

        public DefaultValueAttribute(string field)
        {
            Field = field;
        }
    }
}