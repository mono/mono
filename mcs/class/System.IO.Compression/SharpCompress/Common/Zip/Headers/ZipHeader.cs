using System.IO;

namespace SharpCompress.Common.Zip.Headers
{
    internal abstract class ZipHeader
    {
        protected ZipHeader(ZipHeaderType type)
        {
            ZipHeaderType = type;
            HasData = true;
        }

        internal ZipHeaderType ZipHeaderType { get; private set; }

        internal abstract void Read(BinaryReader reader);

        internal abstract void Write(BinaryWriter writer);

        internal bool HasData { get; set; }
    }
}