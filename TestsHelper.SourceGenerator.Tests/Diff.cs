using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using DiffPlex;
using DiffPlex.Chunkers;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace TestsHelper.SourceGenerator.Tests;

public static class Diff
{
    private static readonly IChunker _lineChunker = new LineChunker();
    private static readonly IChunker _lineEndingsPreservingChunker = new LineEndingsPreservingChunker();
    private static readonly InlineDiffBuilder _diffBuilder = new InlineDiffBuilder(new Differ());

    public static bool IsDifferent(string expected, string actual, [NotNullWhen(true)] out string? displayMessage)
    {
        displayMessage = default;
        if (expected == actual) return false;

        DiffPaneModel? diff = _diffBuilder.BuildDiffModel(expected, actual, ignoreWhitespace: false, ignoreCase: false, _lineChunker);
        var messageBuilder = new StringBuilder();
        messageBuilder.AppendLine("Actual and expected values differ. Expected shown in baseline of diff:");

        if (!diff.Lines.Any(line => line.Type is ChangeType.Inserted or ChangeType.Deleted))
        {
            // We have a failure only caused by line ending differences; recalculate with line endings visible
            diff = _diffBuilder.BuildDiffModel(expected, actual, ignoreWhitespace: false, ignoreCase: false,
                _lineEndingsPreservingChunker);
        }

        foreach (DiffPiece? line in diff.Lines)
        {
            switch (line.Type)
            {
                case ChangeType.Inserted:
                    messageBuilder.Append('+');
                    break;
                case ChangeType.Deleted:
                    messageBuilder.Append('-');
                    break;
                default:
                    messageBuilder.Append(' ');
                    break;
            }

            messageBuilder.AppendLine(line.Text.Replace("\r", "<CR>").Replace("\n", "<LF>"));
        }

        displayMessage = messageBuilder.ToString();
        return true;
    }
}