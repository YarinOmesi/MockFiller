namespace TestsHelper.SourceGenerator.CodeBuilding;

public interface IIndentedStringWriter
{
    public void Write(string str);
    public void WriteSpaceSeperated(params string[] strs);
    public void WriteLine(string str);
    public void WriteLine();
    public void OpenBlock();
    public void CloseBlock();
    public void Indent();
    public void DeIndent();
    public void WriteIndent();
}