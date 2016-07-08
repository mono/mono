using System.IO;
using System.Linq;
using SharpCompress.Common.Zip;

namespace SharpCompress.Archive.Zip
{
    internal class ZipArchiveEntry : ZipEntry, IArchiveEntry
    {
        internal ZipArchiveEntry(ZipArchive archive, SeekableZipFilePart part)
            : base(part)
        {
            Archive = archive;
        }

        public virtual Stream OpenEntryStream()
        {
            return Parts.Single().GetCompressedStream();
        }

        #region IArchiveEntry Members

        public IArchive Archive { get; private set; }

        public bool IsComplete
        {
            get { return true; }
        }

        #endregion

        public string Comment
        {
            get { return (Parts.Single() as SeekableZipFilePart).Comment; }
        }

        public override string ToString()
        {
            return this.Key;
        }
    }
}