using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Types;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TestsHelper.SourceGenerator.CodeBuilding;

public class TypeBuilder : MemberBuilder
{
    private static readonly IReadOnlyList<Type> MembersOrder = new[] {
        typeof(FieldBuilder),
        typeof(PropertyBuilder),
        typeof(ConstructorBuilder),
        typeof(MethodBuilder),
        typeof(TypeBuilder)
    };

    public FileBuilder ParentFileBuilder { get; }
    
    public TypeBuilder? ParentType { get; }

    public string Name { get; set; } = null!;

    public IReadOnlyList<MemberBuilder> Members => _members.Values.SelectMany(list => list).ToList();

    private readonly Dictionary<Type, List<MemberBuilder>> _members = new();

    private readonly string _kind;

    private TypeBuilder(string kind, FileBuilder builder, TypeBuilder? parentType = null)
    {
        _kind = kind;
        ParentFileBuilder = builder;
        ParentType = parentType;
    }

    public void AddMembers(params MemberBuilder[] memberBuilders)
    {
        foreach (MemberBuilder memberBuilder in memberBuilders)
        {
            if (_members.TryGetValue(memberBuilder.GetType(), out var list))
            {
                list.Add(memberBuilder);
            }
            else
            {
                _members[memberBuilder.GetType()] = new List<MemberBuilder> {memberBuilder};
            }
        }
    }

    public override MemberDeclarationSyntax Build(BuildContext context)
    {
        NameSyntax generatedCodeAttributeType = (NameSyntax) context.TryRegisterAlias(CommonTypes.GeneratedCodeAttribute).Build();

        AttributeArgumentSyntax[] attributeArguments = new [] {
            AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(AssemblyInfo.Name))),
            AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(AssemblyInfo.Version.ToString())))
        };
        AttributeSyntax attribute = Attribute(generatedCodeAttributeType)
            .WithArgumentList(AttributeArgumentList(SeparatedList(attributeArguments)));

        AttributeListSyntax attributeListSyntax = AttributeList(SingletonSeparatedList(attribute));
        return ClassDeclaration(Name)
            .AddAttributeLists(attributeListSyntax)
            .WithModifiers(BuildModifiers())
            .AddMembers(MembersOrder.SelectMany(type => _members.TryGetValue(type, out var list)? list : new List<MemberBuilder>())
                .Select(builder => builder.Build(context))
                .ToArray());
    }

    public static TypeBuilder ClassBuilder(FileBuilder fileBuilder) => new("class", fileBuilder);
    public static TypeBuilder ClassBuilder(TypeBuilder parentType) => new("class", parentType.ParentFileBuilder, parentType);
}