using TestsHelper.SourceGenerator.CodeBuilding.Types;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Types;

public static class Moq
{
    public static readonly string Namespace = "Moq";
    public static readonly string MoqLanguageFlow = "Moq.Language.Flow";
    public static readonly NamespacedType Mock = Namespace.Type("Mock");
    public static readonly NamespacedType Times = Namespace.Type("Times");
    public static readonly NamespacedType ISetup = MoqLanguageFlow.Type("ISetup");
}