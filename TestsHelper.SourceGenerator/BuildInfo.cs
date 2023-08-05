using System;
using System.Reflection;

namespace TestsHelper.SourceGenerator;

public static class BuildInfo
{
    private static readonly Assembly _assembly = typeof(BuildInfo).Assembly;

    public static string Name { get; } = _assembly.GetName().Name;
    public static Version Version { get; } = _assembly.GetName().Version;
}