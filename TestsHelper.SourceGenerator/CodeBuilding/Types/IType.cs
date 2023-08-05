using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestsHelper.SourceGenerator.CodeBuilding.Types;

public interface IType
{
    public string Namespace { get; }
    public string Name { get; }

    public TypeSyntax Build();
}

public interface IType<out T> : IType 
    where T : TypeSyntax
{
    public new T Build();
}