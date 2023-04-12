using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TestsHelper.SourceGenerator.FluentSyntaxCreation;

public static partial class Extensions
{
    public static VariableDeclarationSyntax DeclareVariable(this TypeSyntax type, VariableDeclaratorSyntax variable)
    {
        return VariableDeclaration(type, SingletonSeparatedList(variable));
    }

    public static VariableDeclarationSyntax DeclareVariable(this TypeSyntax type, string name, ExpressionSyntax? initializer = null)
    {
        if (initializer == null)
            return type.DeclareVariable(VariableDeclarator(name));

        return type.DeclareVariable(VariableDeclarator(name).WithInitializer(EqualsValueClause(initializer)));
    }

    public static VariableDeclarationSyntax DeclareVariable(this string name, ExpressionSyntax initializer) =>
        IdentifierName(VarIdentifier).DeclareVariable(name, initializer);

    public static LocalDeclarationStatementSyntax ToStatement(this VariableDeclarationSyntax declarationSyntax) => 
        LocalDeclarationStatement(declarationSyntax);

}