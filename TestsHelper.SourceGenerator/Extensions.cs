using System.Collections.Generic;

namespace TestsHelper.SourceGenerator;

public static class Extensions
{
    public static string JoinToString(this IEnumerable<string> values, string separator) => string.Join(separator, values);
}