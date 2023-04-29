using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsHelper.SourceGenerator.CodeBuilding.Abstractions;

namespace TestsHelper.SourceGenerator.CodeBuilding;

public class TypeBuilder : MemberBuilder, ITypeBuilder
{
    private static readonly IReadOnlyList<Type> MembersOrder = new[] {
        typeof(FieldBuilder),
        typeof(PropertyBuilder),
        typeof(ConstructorBuilder),
        typeof(MethodBuilder),
        typeof(TypeBuilder)
    };

    public IFileBuilder ParentFileBuilder { get; }

    public string Name { get; set; } = null!;

    public IReadOnlyList<IMemberBuilder> Members => _members.Values.SelectMany(list => list).ToList();

    private readonly Dictionary<Type, List<IMemberBuilder>> _members = new();

    private readonly string _kind;

    private TypeBuilder(string kind, IFileBuilder builder)
    {
        _kind = kind;
        ParentFileBuilder = builder;
    }

    public void AddMembers(params IMemberBuilder[] memberBuilders)
    {
        foreach (IMemberBuilder memberBuilder in memberBuilders)
        {
            if (_members.TryGetValue(memberBuilder.GetType(), out var list))
            {
                list.Add(memberBuilder);
            }
            else
            {
                _members[memberBuilder.GetType()] = new List<IMemberBuilder> {memberBuilder};
            }
        }
    }

    public override MemberDeclarationSyntax Build()
    {
        return SyntaxFactory.ClassDeclaration(Name)
            .WithModifiers(BuildModifiers())
            .AddMembers(MembersOrder.SelectMany(type => _members.TryGetValue(type, out var list)? list : new List<IMemberBuilder>())
                .Select(builder => builder.Build())
                .ToArray());
    }

    public static TypeBuilder ClassBuilder(IFileBuilder fileBuilder) => new("class", fileBuilder);
}