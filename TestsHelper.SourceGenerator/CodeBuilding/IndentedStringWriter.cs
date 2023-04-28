using System.IO;
using System.Linq;

namespace TestsHelper.SourceGenerator.CodeBuilding;

public class IndentedStringWriter : IIndentedStringWriter
{
    private readonly StringWriter _stringWriter;
    private int _indentLevel = 0;
    private readonly string _indent;


    public IndentedStringWriter(StringWriter stringWriter, string indent)
    {
        _stringWriter = stringWriter;
        _indent = indent;
    }

    public void Write(string str)
    {
        _stringWriter.Write(str);
    }

    public void WriteSpaceSeperated(params string[] strs)
    {
        _stringWriter.Write(strs.Where(s => !string.IsNullOrEmpty(s)).JoinToString(" "));
    }

    public void WriteLine(string str)
    {
        WriteIndent();
        _stringWriter.WriteLine(str);
    }

    public void WriteLine() => _stringWriter.WriteLine();


    public void OpenBlock()
    {
        WriteLine("{");
        _indentLevel++;
    }

    public void CloseBlock()
    {
        _indentLevel--;
        WriteLine("}");
    }

    public void WriteIndent()
    {
        for (var i = 0; i < _indentLevel; i++)
        {
            _stringWriter.Write(_indent);
        }
    }
}