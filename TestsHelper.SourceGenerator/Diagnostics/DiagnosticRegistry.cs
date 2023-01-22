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
}