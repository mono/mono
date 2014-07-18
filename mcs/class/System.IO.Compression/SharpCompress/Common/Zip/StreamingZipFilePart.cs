using System;
using System.IO;
using System.IO.Compression;
using SharpCompress.Common.Zip.Headers;
using SharpCompress.IO;

namespace SharpCompress.Common.Zip
{
    internal class StreamingZipFilePart : ZipFilePart
    {
        private Stream decompressionStream;

        internal StreamingZipFilePart(ZipFileEntry header, Stream stream)
            : base(header, stream)
        {
        }

        protected override Stream CreateBaseStream()
        {
            return Header.PackedStream;
        }

        internal override Stream GetCompressedStream()
        {
            if (!Header.HasData)
            {
                return Stream.Null;
            }
            decompressionStream = CreateDecompressionStream(GetCryptoStream(CreateBaseStream()));
            if (LeaveStreamOpen)
            {
                return new NonDisposingStream(decompressionStream);
            }
            return decompressionStream;
        }

        internal BinaryReader FixStreamedFileLocation(ref RewindableStream rewindableStream)
        {
            if (Header.IsDirectory)
            {
                return new BinaryReader(rewindableStream);
            }
            if (Header.HasData)
            {
                if (decompressionStream == null)
                {
                    decompressionStream = GetCompressedStream();
                }
                decompressionStream.SkipAll();

                DeflateStream deflateStream = decompressionStream as DeflateStream;
                if (deflateStream != null)
                {
                    rewindableStream.Rewind(deflateStream.BaseStream as MemoryStream);
                }
            }
            var reader = new BinaryReader(rewindableStream);
            decompressionStream = null;
            return reader;
        }
    }
}