using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SharpCompress.Common;
using SharpCompress.Common.Zip;
using SharpCompress.Common.Zip.Headers;
using SharpCompress.Compressor;
#if BZIP2
using SharpCompress.Compressor.BZip2;
#endif
using SharpCompress.Compressor.Deflate;
#if LZMA
using SharpCompress.Compressor.LZMA;
#endif
#if PPMd
using SharpCompress.Compressor.PPMd;
#endif
using SharpCompress.IO;
#if DEFLATE
using DeflateStream = SharpCompress.Compressor.Deflate.DeflateStream;
#endif

namespace SharpCompress.Writer.Zip
{
    internal class ZipWriter : AbstractWriter
    {
        private readonly ZipCompressionMethod compression;
        private readonly CompressionLevel deflateCompressionLevel;

        private readonly List<ZipCentralDirectoryEntry> entries = new List<ZipCentralDirectoryEntry>();
        private readonly string zipComment;
        private readonly Encoding encoding;
        private long streamPosition;

#if PPMd
        private readonly PpmdProperties ppmdProperties; // Caching properties to speed up PPMd.
#endif

        public ZipWriter(Stream destination, CompressionInfo compressionInfo, string zipComment, Encoding encoding = null)
            : base(ArchiveType.Zip)
        {
            this.zipComment = zipComment ?? string.Empty;
            this.encoding = encoding ?? ArchiveEncoding.Default;

            switch (compressionInfo.Type)
            {
                case CompressionType.None:
                    {
                        compression = ZipCompressionMethod.None;
                    }
                    break;
                case CompressionType.Deflate:
                    {
                        compression = ZipCompressionMethod.Deflate;
                        deflateCompressionLevel = compressionInfo.DeflateCompressionLevel;
                    }
                    break;
                case CompressionType.BZip2:
                    {
                        compression = ZipCompressionMethod.BZip2;
                    }
                    break;
#if LZMA
                case CompressionType.LZMA:
                    {
                        compression = ZipCompressionMethod.LZMA;
                    }
                    break;
#endif
#if PPMd
                case CompressionType.PPMd:
                    {
                        ppmdProperties = new PpmdProperties();
                        compression = ZipCompressionMethod.PPMd;
                    }
                    break;
#endif
                default:
                    throw new InvalidFormatException("Invalid compression method: " + compressionInfo.Type);
            }
            InitalizeStream(destination, false);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                uint size = 0;
                foreach (ZipCentralDirectoryEntry entry in entries)
                {
                    size += entry.Write(OutputStream, compression);
                }
                WriteEndRecord(size);
            }
            base.Dispose(isDisposing);
        }

        public override void Write(string entryPath, Stream source, DateTime? modificationTime)
        {
            Write(entryPath, source, modificationTime, null);
        }

        public void Write(string entryPath, Stream source, DateTime? modificationTime, string comment)
        {
            using (Stream output = WriteToStream(entryPath, modificationTime, comment))
            {
                source.TransferTo(output);
            }
        }

        public Stream WriteToStream(string entryPath, DateTime? modificationTime, string comment)
        {
            entryPath = NormalizeFilename(entryPath);
            modificationTime = modificationTime ?? DateTime.Now;
            comment = comment ?? "";
            var entry = new ZipCentralDirectoryEntry
                            {
                                Comment = comment,
                                FileName = entryPath,
                                ModificationTime = modificationTime,
                                HeaderOffset = (uint) streamPosition,
                            };
            var headersize = (uint) WriteHeader(entryPath, modificationTime);
            streamPosition += headersize;
            return new ZipWritingStream(this, OutputStream, entry);
        }

        private string NormalizeFilename(string filename)
        {
            filename = filename.Replace('\\', '/');

            int pos = filename.IndexOf(':');
            if (pos >= 0)
                filename = filename.Remove(0, pos + 1);

            return filename;
        }

        private int WriteHeader(string filename, DateTime? modificationTime)
        {
            byte[] encodedFilename = encoding.GetBytes(filename);

            OutputStream.Write(BitConverter.GetBytes(ZipHeaderFactory.ENTRY_HEADER_BYTES), 0, 4);
            OutputStream.Write(new byte[] {20, 0}, 0, 2); //version
            HeaderFlags flags = encoding == Encoding.UTF8 ? HeaderFlags.UTF8 : (HeaderFlags)0;
            if (!OutputStream.CanSeek)
            {
                flags |= HeaderFlags.UsePostDataDescriptor;
                if (compression == ZipCompressionMethod.LZMA)
                {
                    flags |= HeaderFlags.Bit1; // eos marker
                }
            }
            OutputStream.Write(BitConverter.GetBytes((ushort) flags), 0, 2);
            OutputStream.Write(BitConverter.GetBytes((ushort) compression), 0, 2); // zipping method
            OutputStream.Write(BitConverter.GetBytes(modificationTime.DateTimeToDosTime()), 0, 4);
            // zipping date and time
            OutputStream.Write(new byte[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, 0, 12);
            // unused CRC, un/compressed size, updated later
            OutputStream.Write(BitConverter.GetBytes((ushort) encodedFilename.Length), 0, 2); // filename length
            OutputStream.Write(BitConverter.GetBytes((ushort) 0), 0, 2); // extra length
            OutputStream.Write(encodedFilename, 0, encodedFilename.Length);

            return 6 + 2 + 2 + 4 + 12 + 2 + 2 + encodedFilename.Length;
        }

        private void WriteFooter(uint crc, uint compressed, uint uncompressed)
        {
            OutputStream.Write(BitConverter.GetBytes(crc), 0, 4);
            OutputStream.Write(BitConverter.GetBytes(compressed), 0, 4);
            OutputStream.Write(BitConverter.GetBytes(uncompressed), 0, 4);
        }

        private void WriteEndRecord(uint size)
        {
            byte[] encodedComment = encoding.GetBytes(zipComment);

            OutputStream.Write(new byte[] {80, 75, 5, 6, 0, 0, 0, 0}, 0, 8);
            OutputStream.Write(BitConverter.GetBytes((ushort) entries.Count), 0, 2);
            OutputStream.Write(BitConverter.GetBytes((ushort) entries.Count), 0, 2);
            OutputStream.Write(BitConverter.GetBytes(size), 0, 4);
            OutputStream.Write(BitConverter.GetBytes((uint) streamPosition), 0, 4);
            OutputStream.Write(BitConverter.GetBytes((ushort) encodedComment.Length), 0, 2);
            OutputStream.Write(encodedComment, 0, encodedComment.Length);
        }

        #region Nested type: ZipWritingStream

        internal class ZipWritingStream : Stream
        {
            private readonly CRC32 crc = new CRC32();
            private readonly ZipCentralDirectoryEntry entry;
            private readonly Stream originalStream;
            private readonly Stream writeStream;
            private readonly ZipWriter writer;
            private CountingWritableSubStream counting;
            private uint decompressed;

            internal ZipWritingStream(ZipWriter writer, Stream originalStream, ZipCentralDirectoryEntry entry)
            {
                this.writer = writer;
                this.originalStream = originalStream;
                writeStream = GetWriteStream(originalStream);
                this.writer = writer;
                this.entry = entry;
            }

            public override bool CanRead
            {
                get { return false; }
            }

            public override bool CanSeek
            {
                get { return false; }
            }

            public override bool CanWrite
            {
                get { return true; }
            }

            public override long Length
            {
                get { throw new NotSupportedException(); }
            }

            public override long Position
            {
                get { throw new NotSupportedException(); }
                set { throw new NotSupportedException(); }
            }

            private Stream GetWriteStream(Stream writeStream)
            {
                counting = new CountingWritableSubStream(writeStream);
                Stream output = counting;
                switch (writer.compression)
                {
                    case ZipCompressionMethod.None:
                        {
                            return output;
                        }
                    case ZipCompressionMethod.Deflate:
                        {
                            return new System.IO.Compression.DeflateStream(counting,
                                System.IO.Compression.CompressionMode.Compress, true);
                        }
#if BZIP2
                    case ZipCompressionMethod.BZip2:
                        {
                            return new BZip2Stream(counting, CompressionMode.Compress, true);
                        }
#endif
#if LZMA
                    case ZipCompressionMethod.LZMA:
                        {
                            counting.WriteByte(9);
                            counting.WriteByte(20);
                            counting.WriteByte(5);
                            counting.WriteByte(0);

                            LzmaStream lzmaStream = new LzmaStream(new LzmaEncoderProperties(!originalStream.CanSeek),
                                                                   false, counting);
                            counting.Write(lzmaStream.Properties, 0, lzmaStream.Properties.Length);
                            return lzmaStream;
                        }
#endif
#if PPMd
                    case ZipCompressionMethod.PPMd:
                        {
                            counting.Write(writer.ppmdProperties.Properties, 0, 2);
                            return new PpmdStream(writer.ppmdProperties, counting, true);
                        }
#endif
                    default:
                        {
                            throw new NotSupportedException("CompressionMethod: " + writer.compression);
                        }
                }
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                if (disposing)
                {
                    writeStream.Dispose();
                    entry.Crc = (uint) crc.Crc32Result;
                    entry.Compressed = counting.Count;
                    entry.Decompressed = decompressed;
                    if (originalStream.CanSeek)
                    {
                        originalStream.Position = entry.HeaderOffset + 6;
                        originalStream.WriteByte(0);
                        originalStream.Position = entry.HeaderOffset + 14;
                        writer.WriteFooter(entry.Crc, counting.Count, decompressed);
                        originalStream.Position = writer.streamPosition + entry.Compressed;
                        writer.streamPosition += entry.Compressed;
                    }
                    else
                    {
                        originalStream.Write(BitConverter.GetBytes(ZipHeaderFactory.POST_DATA_DESCRIPTOR), 0, 4);
                        writer.WriteFooter(entry.Crc, counting.Count, decompressed);
                        writer.streamPosition += entry.Compressed + 16;
                    }
                    writer.entries.Add(entry);
                }
            }

            public override void Flush()
            {
                writeStream.Flush();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                decompressed += (uint) count;
                crc.SlurpBlock(buffer, offset, count);
                writeStream.Write(buffer, offset, count);
            }
        }

        #endregion
    }
}