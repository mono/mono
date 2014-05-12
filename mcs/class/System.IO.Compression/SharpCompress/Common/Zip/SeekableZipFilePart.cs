using System.IO;
using SharpCompress.Common.Zip.Headers;

namespace SharpCompress.Common.Zip
{
    internal class SeekableZipFilePart : ZipFilePart
    {
        private bool isLocalHeaderLoaded;
        private readonly SeekableZipHeaderFactory headerFactory;

        internal SeekableZipFilePart(SeekableZipHeaderFactory headerFactory, DirectoryEntryHeader header, Stream stream)
            : base(header, stream)
        {
            this.headerFactory = headerFactory;
        }

        internal override Stream GetCompressedStream()
        {
            if (!isLocalHeaderLoaded)
            {
                LoadLocalHeader();
                isLocalHeaderLoaded = true;
            }
            return base.GetCompressedStream();
        }

        internal string Comment
        {
            get { return (Header as DirectoryEntryHeader).Comment; }
        }

        private void LoadLocalHeader()
        {
            bool hasData = Header.HasData;
            Header = headerFactory.GetLocalHeader(BaseStream, Header as DirectoryEntryHeader);
            Header.HasData = hasData;
        }

        protected override Stream CreateBaseStream()
        {
            BaseStream.Position = Header.DataStartPosition.Value;
            return BaseStream;
        }
    }
}