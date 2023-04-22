using System.Collections.Generic;
using TestsHelper.SourceGenerator.CodeBuilding.Abstractions;

namespace TestsHelper.SourceGenerator.CodeBuilding;

public static class Writer
{
    public static class Block
    {
        public static void Write(IIndentedStringWriter writer, IEnumerable<IWritable> writables)
        {
            writer.OpenBlock();
            foreach (IWritable writable in writables)
            {
                writer.WriteIndent();
                writable.Write(writer);
            }

            writer.CloseBlock();
        }

        public static void Write(IIndentedStringWriter writer, IEnumerable<string> lines)
        {
            writer.OpenBlock();
            foreach (string line in lines)
            {
                writer.WriteLine(line);
            }

            writer.CloseBlock();
        }
    }

    public static class CommaSeperated
    {
        public static void Write(IIndentedStringWriter writer, IEnumerable<string> strings)
        {
            bool addComma = false;
            foreach (string str in strings)
            {
                if (addComma)
                {
                    writer.Write(", ");
                }

                writer.Write(str);
                addComma = true;
            }
        }

        public static void Write(IIndentedStringWriter writer, IEnumerable<IWritable> builders)
        {
            bool addComma = false;
            foreach (var writable in builders)
            {
                if (addComma)
                {
                    writer.Write(", ");
                }

                writable.Write(writer);
                addComma = true;
            }
        }
    }
}