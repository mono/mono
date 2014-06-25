using System.IO;

namespace SharpCompress.Archive
{
    internal interface IWritableArchiveEntry
    {
        Stream Stream { get; }
    }
}