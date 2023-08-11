using System;
using System.Reflection;

namespace TestsHelper.SourceGenerator;

public static class AssemblyInfo
{
    private static readonly Assembly _assembly = typeof(AssemblyInfo).Assembly;

    public static string Name { get; } = _assembly.GetName().Name;
    public static Version Version { get; } = _assembly.GetName().Version;
}