using System;
using System.Collections.Generic;
using System.IO;
using SharpCompress.Common.Zip.Headers;
using SharpCompress.IO;

namespace SharpCompress.Common.Zip
{
    internal class SeekableZipHeaderFactory : ZipHeaderFactory
    {
        private const int MAX_ITERATIONS_FOR_DIRECTORY_HEADER = 1000;

        internal SeekableZipHeaderFactory(string password)
            : base(StreamingMode.Seekable, password)
        {
        }

        internal IEnumerable<DirectoryEntryHeader> ReadSeekableHeader(Stream stream)
        {
            long offset = 0;
            uint signature;
            BinaryReader reader = new BinaryReader(stream);

            int iterationCount = 0;
            do
            {
                if ((stream.Length + offset) - 4 < 0)
                {
                    throw new ArchiveException("Failed to locate the Zip Header");
                }
                stream.Seek(offset - 4, SeekOrigin.End);
                signature = reader.ReadUInt32();
                offset--;
                iterationCount++;
                if (iterationCount > MAX_ITERATIONS_FOR_DIRECTORY_HEADER)
                {
                    throw new ArchiveException(
                        "Could not find Zip file Directory at the end of the file.  File may be corrupted.");
                }
            } while (signature != DIRECTORY_END_HEADER_BYTES);

            var entry = new DirectoryEndHeader();
            entry.Read(reader);
            stream.Seek(entry.DirectoryStartOffsetRelativeToDisk, SeekOrigin.Begin);

            DirectoryEntryHeader directoryEntryHeader = null;
            long position = stream.Position;
            while (true)
            {
                stream.Position = position;
                signature = reader.ReadUInt32();
                directoryEntryHeader = ReadHeader(signature, reader) as DirectoryEntryHeader;
                position = stream.Position;
                if (directoryEntryHeader == null)
                {
                    yield break;
                }
                //entry could be zero bytes so we need to know that.
                directoryEntryHeader.HasData = directoryEntryHeader.CompressedSize != 0;
                yield return directoryEntryHeader;
            }
        }

        internal LocalEntryHeader GetLocalHeader(Stream stream, DirectoryEntryHeader directoryEntryHeader)
        {
            stream.Seek(directoryEntryHeader.RelativeOffsetOfEntryHeader, SeekOrigin.Begin);
            BinaryReader reader = new BinaryReader(stream);
            uint signature = reader.ReadUInt32();
            var localEntryHeader = ReadHeader(signature, reader) as LocalEntryHeader;
            if (localEntryHeader == null)
            {
                throw new InvalidOperationException();
            }
            return localEntryHeader;
        }
    }
}