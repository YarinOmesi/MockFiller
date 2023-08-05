using System.Collections.Generic;

namespace TestsHelper.SourceGenerator;

public static class EmptyList<T>
{
    public static List<T> Instance { get; } = new List<T>(0);
}