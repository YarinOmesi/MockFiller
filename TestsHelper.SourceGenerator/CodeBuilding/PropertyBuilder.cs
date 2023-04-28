using TestsHelper.SourceGenerator.CodeBuilding.Types;

namespace TestsHelper.SourceGenerator.CodeBuilding;

internal class PropertyBuilder : FieldBuilder
{
    public bool AutoGetter { get; set; } = true;

    public bool AutoSetter { get; set; } = true;

    private PropertyBuilder() { }

    public override void Write(IIndentedStringWriter writer)
    {
        WriteModifiers(writer);
        WriteTypeAndName(writer);
        WriteInitializer(writer);
        
        writer.Write(" { ");
        if(AutoGetter) writer.Write("get;");
        if(AutoSetter) writer.Write("set;");
        writer.Write(" }");
        writer.WriteLine();
    }

    public static PropertyBuilder Create(IType type, string name, string? initializer = null, bool autoGetter = false, bool autoSetter = false)
    {
        return new PropertyBuilder()
        {
            Type = type, Name = name, Initializer = initializer, AutoGetter = autoGetter, AutoSetter = autoSetter
        };
    }
}