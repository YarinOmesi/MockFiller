using TestsHelper.SourceGenerator.CodeBuilding.Types;

namespace TestsHelper.SourceGenerator.MockFilling.PartialImplementation.Types;

public static class Moq
{
    public static readonly string Namespace = "Moq";
    public static readonly string MoqLanguageFlow = "Moq.Language.Flow";
    public static readonly RegularType Mock = Namespace.Type("Mock");
    public static readonly RegularType Times = Namespace.Type("Times");
    public static readonly RegularType ISetup = MoqLanguageFlow.Type("ISetup");
}