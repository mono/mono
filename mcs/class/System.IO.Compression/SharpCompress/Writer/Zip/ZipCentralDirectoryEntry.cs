using System;
using System.IO;
using System.Text;
using SharpCompress.Common.Zip;
using SharpCompress.Common.Zip.Headers;

namespace SharpCompress.Writer.Zip
{
    internal class ZipCentralDirectoryEntry
    {
        internal string FileName { get; set; }
        internal DateTime? ModificationTime { get; set; }
        internal string Comment { get; set; }
        internal uint Crc { get; set; }
        internal uint HeaderOffset { get; set; }
        internal uint Compressed { get; set; }
        internal uint Decompressed { get; set; }


        internal uint Write(Stream outputStream, ZipCompressionMethod compression)
        {
            byte[] encodedFilename = Encoding.UTF8.GetBytes(FileName);
            byte[] encodedComment = Encoding.UTF8.GetBytes(Comment);

            outputStream.Write(new byte[] {80, 75, 1, 2, 0x14, 0, 0x0A, 0}, 0, 8);
            HeaderFlags flags = HeaderFlags.UTF8;
            if (!outputStream.CanSeek)
            {
                flags |= HeaderFlags.UsePostDataDescriptor;
                if (compression == ZipCompressionMethod.LZMA)
                {
                    flags |= HeaderFlags.Bit1; // eos marker
                }
            }
            outputStream.Write(BitConverter.GetBytes((ushort) flags), 0, 2);
            outputStream.Write(BitConverter.GetBytes((ushort) compression), 0, 2); // zipping method
            outputStream.Write(BitConverter.GetBytes(ModificationTime.DateTimeToDosTime()), 0, 4);
                // zipping date and time
            outputStream.Write(BitConverter.GetBytes(Crc), 0, 4); // file CRC
            outputStream.Write(BitConverter.GetBytes(Compressed), 0, 4); // compressed file size
            outputStream.Write(BitConverter.GetBytes(Decompressed), 0, 4); // uncompressed file size
            outputStream.Write(BitConverter.GetBytes((ushort) encodedFilename.Length), 0, 2); // Filename in zip
            outputStream.Write(BitConverter.GetBytes((ushort) 0), 0, 2); // extra length
            outputStream.Write(BitConverter.GetBytes((ushort) encodedComment.Length), 0, 2);

            outputStream.Write(BitConverter.GetBytes((ushort) 0), 0, 2); // disk=0
            outputStream.Write(BitConverter.GetBytes((ushort) 0), 0, 2); // file type: binary
            outputStream.Write(BitConverter.GetBytes((ushort) 0), 0, 2); // Internal file attributes
            outputStream.Write(BitConverter.GetBytes((ushort) 0x8100), 0, 2);
                // External file attributes (normal/readable)
            outputStream.Write(BitConverter.GetBytes(HeaderOffset), 0, 4); // Offset of header

            outputStream.Write(encodedFilename, 0, encodedFilename.Length);
            outputStream.Write(encodedComment, 0, encodedComment.Length);

            return (uint) (8 + 2 + 2 + 4 + 4 + 4 + 4 + 2 + 2 + 2
                           + 2 + 2 + 2 + 2 + 4 + encodedFilename.Length + encodedComment.Length);
        }
    }
}