using System.Collections.Generic;
using System.IO;
using SharpCompress.Common.Zip.Headers;
using SharpCompress.IO;

namespace SharpCompress.Common.Zip
{
    internal class StreamingZipHeaderFactory : ZipHeaderFactory
    {
        internal StreamingZipHeaderFactory(string password)
            : base(StreamingMode.Streaming, password)
        {
        }

        internal IEnumerable<ZipHeader> ReadStreamHeader(Stream stream)
        {
            RewindableStream rewindableStream;
            if (stream is RewindableStream)
            {
                rewindableStream = stream as RewindableStream;
            }
            else
            {
                rewindableStream = new RewindableStream(stream);
            }
            while (true)
            {
                ZipHeader header = null;
                BinaryReader reader = new BinaryReader(rewindableStream);
                if (lastEntryHeader != null &&
                    FlagUtility.HasFlag(lastEntryHeader.Flags, HeaderFlags.UsePostDataDescriptor))
                {
                    reader = (lastEntryHeader.Part as StreamingZipFilePart).FixStreamedFileLocation(ref rewindableStream);
                    long pos = rewindableStream.Position;
                    uint crc = reader.ReadUInt32();
                    if (crc == POST_DATA_DESCRIPTOR)
                    {
                        crc = reader.ReadUInt32();
                    }
                    lastEntryHeader.Crc = crc;
                    lastEntryHeader.CompressedSize = reader.ReadUInt32();
                    lastEntryHeader.UncompressedSize = reader.ReadUInt32();
                    lastEntryHeader.DataStartPosition = pos - lastEntryHeader.CompressedSize;
                }
                lastEntryHeader = null;
                uint headerBytes = reader.ReadUInt32();
                header = ReadHeader(headerBytes, reader);

                //entry could be zero bytes so we need to know that.
                if (header.ZipHeaderType == ZipHeaderType.LocalEntry)
                {
                    bool isRecording = rewindableStream.IsRecording;
                    if (!isRecording)
                    {
                        rewindableStream.StartRecording();
                    }
                    uint nextHeaderBytes = reader.ReadUInt32();
                    header.HasData = !IsHeader(nextHeaderBytes);
                    rewindableStream.Rewind(!isRecording);
                }
                yield return header;
            }
        }
    }
}