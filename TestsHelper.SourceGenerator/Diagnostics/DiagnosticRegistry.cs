using Microsoft.CodeAnalysis;

namespace TestsHelper.SourceGenerator.Diagnostics;

public static class DiagnosticRegistry
{
    private const string Category = "MockFiller";

    public static readonly DiagnosticDescriptor ClassIsNotPartial = new(
        id: "TH0001",
        title: "Class Is Not Marked Partial",
        messageFormat: "Cannot Generate Code For Class {0}. Class Not Mark As Partial.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor MoreThanOneFillMockUsage = new(
        id: "TH0002",
        title: "FillMocks Can Be Used Once In Class",
        messageFormat: "Only One [FillMocks] is supported.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor DefaultValueToUnknownParameter = new(
        id: "TH0003",
        title: "Default Value For Unknown Parameter",
        messageFormat: "Parameter Named '{0}' not exists.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor DefaultValueWithWrongType = new(
        id: "TH0004",
        title: "Default Value With Wrong Parameter Type",
        messageFormat: "Cant Use Type '{0}' For Default Value To Parameter Named '{1}' Of Type '{2}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
}