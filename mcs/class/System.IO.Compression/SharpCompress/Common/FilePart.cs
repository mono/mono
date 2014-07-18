using System.IO;

namespace SharpCompress.Common
{
    internal abstract class FilePart
    {
        internal abstract string FilePartName { get; }

        internal abstract Stream GetCompressedStream();
        internal abstract Stream GetRawStream();
    }
}