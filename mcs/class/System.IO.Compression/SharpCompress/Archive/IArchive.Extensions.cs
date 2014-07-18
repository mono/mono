using System.Linq;
using SharpCompress.Common;

namespace SharpCompress.Archive
{
    internal static class IArchiveExtensions
    {
#if !PORTABLE && !NETFX_CORE
        /// <summary>
        /// Extract to specific directory, retaining filename
        /// </summary>
        public static void WriteToDirectory(this IArchive archive, string destinationDirectory,
                                            ExtractOptions options = ExtractOptions.Overwrite)
        {
            foreach (IArchiveEntry entry in archive.Entries.Where(x => !x.IsDirectory))
            {
                entry.WriteToDirectory(destinationDirectory, options);
            }
        }
#endif
    }
}