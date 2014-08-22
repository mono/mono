using System;
using System.IO;

namespace SharpCompress.Writer
{
    internal static class IWriterExtensions
    {
        public static void Write(this IWriter writer, string entryPath, Stream source)
        {
            writer.Write(entryPath, source, null);
        }

#if !PORTABLE && !NETFX_CORE
        public static void Write(this IWriter writer, string entryPath, FileInfo source)
        {
            if (!source.Exists)
            {
                throw new ArgumentException("Source does not exist: " + source.FullName);
            }
            using (var stream = source.OpenRead())
            {
                writer.Write(entryPath, stream, source.LastWriteTime);
            }
        }

        public static void Write(this IWriter writer, string entryPath, string source)
        {
            writer.Write(entryPath, new FileInfo(source));
        }

        public static void WriteAll(this IWriter writer, string directory, string searchPattern = "*",
                                    SearchOption option = SearchOption.TopDirectoryOnly)
        {
            if (!Directory.Exists(directory))
            {
                throw new ArgumentException("Directory does not exist: " + directory);
            }
#if NET2
            foreach (var file in Directory.GetFiles(directory, searchPattern, option))
#else
            foreach (var file in Directory.EnumerateFiles(directory, searchPattern, option))
#endif
            {
                writer.Write(file.Substring(directory.Length), file);
            }
        }
#endif
    }
}