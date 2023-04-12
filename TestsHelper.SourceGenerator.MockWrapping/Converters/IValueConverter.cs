using System.Linq.Expressions;

namespace TestsHelper.SourceGenerator.MockWrapping.Converters;

public interface IValueConverter
{
    /// <summary>
    /// convert value to expression
    /// </summary>
    /// <param name="value">value to convert this is default when user not specify value should be any</param>
    /// <typeparam name="T"></typeparam>
    /// <returns>Expression that represent the value</returns>
    public Expression Convert<T>(Value<T> value);
}