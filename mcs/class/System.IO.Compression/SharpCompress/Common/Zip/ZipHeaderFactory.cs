using System;
using System.IO;
using SharpCompress.Common.Zip.Headers;
using SharpCompress.IO;
#if !PORTABLE
using System.Linq;

#endif

namespace SharpCompress.Common.Zip
{
    internal class ZipHeaderFactory
    {
        internal const uint ENTRY_HEADER_BYTES = 0x04034b50;
        internal const uint POST_DATA_DESCRIPTOR = 0x08074b50;
        internal const uint DIRECTORY_START_HEADER_BYTES = 0x02014b50;
        internal const uint DIRECTORY_END_HEADER_BYTES = 0x06054b50;
        internal const uint DIGITAL_SIGNATURE = 0x05054b50;
        internal const uint SPLIT_ARCHIVE_HEADER_BYTES = 0x30304b50;

        private const uint ZIP64_END_OF_CENTRAL_DIRECTORY = 0x06064b50;
        private const uint ZIP64_END_OF_CENTRAL_DIRECTORY_LOCATOR = 0x07064b50;


        protected LocalEntryHeader lastEntryHeader;
        private string password;
        private StreamingMode mode;

        protected ZipHeaderFactory(StreamingMode mode, string password)
        {
            this.mode = mode;
            this.password = password;
        }

        protected ZipHeader ReadHeader(uint headerBytes, BinaryReader reader)
        {
            switch (headerBytes)
            {
                case ENTRY_HEADER_BYTES:
                    {
                        var entryHeader = new LocalEntryHeader();
                        entryHeader.Read(reader);
                        LoadHeader(entryHeader, reader.BaseStream);

                        lastEntryHeader = entryHeader;
                        return entryHeader;
                    }
                case DIRECTORY_START_HEADER_BYTES:
                    {
                        var entry = new DirectoryEntryHeader();
                        entry.Read(reader);
                        return entry;
                    }
                case POST_DATA_DESCRIPTOR:
                    {
                        if (FlagUtility.HasFlag(lastEntryHeader.Flags, HeaderFlags.UsePostDataDescriptor))
                        {
                            lastEntryHeader.Crc = reader.ReadUInt32();
                            lastEntryHeader.CompressedSize = reader.ReadUInt32();
                            lastEntryHeader.UncompressedSize = reader.ReadUInt32();
                        }
                        else
                        {
                            reader.ReadUInt32();
                            reader.ReadUInt32();
                            reader.ReadUInt32();
                        }
                        return null;
                    }
                case DIGITAL_SIGNATURE:
                    return null;
                case DIRECTORY_END_HEADER_BYTES:
                    {
                        var entry = new DirectoryEndHeader();
                        entry.Read(reader);
                        return entry;
                    }
                case SPLIT_ARCHIVE_HEADER_BYTES:
                    {
                        return new SplitHeader();
                    }
                case ZIP64_END_OF_CENTRAL_DIRECTORY:
                case ZIP64_END_OF_CENTRAL_DIRECTORY_LOCATOR:
                    {
                        var entry = new IgnoreHeader(ZipHeaderType.Ignore);
                        entry.Read(reader);
                        return entry;
                    }
                default:
                    throw new NotSupportedException("Unknown header: " + headerBytes);
            }
        }

        internal static bool IsHeader(uint headerBytes)
        {
            switch (headerBytes)
            {
                case ENTRY_HEADER_BYTES:
                case DIRECTORY_START_HEADER_BYTES:
                case POST_DATA_DESCRIPTOR:
                case DIGITAL_SIGNATURE:
                case DIRECTORY_END_HEADER_BYTES:
                case SPLIT_ARCHIVE_HEADER_BYTES:
                case ZIP64_END_OF_CENTRAL_DIRECTORY:
                case ZIP64_END_OF_CENTRAL_DIRECTORY_LOCATOR:
                    return true;
                default:
                    return false;
            }
        }

        private void LoadHeader(ZipFileEntry entryHeader, Stream stream)
        {
            if (FlagUtility.HasFlag(entryHeader.Flags, HeaderFlags.Encrypted))
            {
                if (!entryHeader.IsDirectory &&
                    entryHeader.CompressedSize == 0 &&
                    FlagUtility.HasFlag(entryHeader.Flags, HeaderFlags.UsePostDataDescriptor))
                {
                    throw new NotSupportedException(
                        "SharpCompress cannot currently read non-seekable Zip Streams with encrypted data that has been written in a non-seekable manner.");
                }
                if (password == null)
                {
                    throw new CryptographicException("No password supplied for encrypted zip.");
                }
                if (entryHeader.CompressionMethod != ZipCompressionMethod.WinzipAes)
                {
                    byte[] buffer = new byte[12];
                    stream.Read(buffer, 0, 12);
                    entryHeader.PkwareTraditionalEncryptionData = PkwareTraditionalEncryptionData.ForRead(password,
                                                                                                          entryHeader,
                                                                                                          buffer);
                    entryHeader.CompressedSize -= 12;
                }
                else
                {
#if PORTABLE || NETFX_CORE
                    throw new NotSupportedException("Cannot decrypt Winzip AES with Silverlight or WP7.");
#else

                    var data = entryHeader.Extra.Where(x => x.Type == ExtraDataType.WinZipAes).SingleOrDefault();
                    WinzipAesKeySize keySize = (WinzipAesKeySize) data.DataBytes[4];

                    byte[] salt = new byte[WinzipAesEncryptionData.KeyLengthInBytes(keySize)/2];
                    byte[] passwordVerifyValue = new byte[2];
                    stream.Read(salt, 0, salt.Length);
                    stream.Read(passwordVerifyValue, 0, 2);
                    entryHeader.WinzipAesEncryptionData = new WinzipAesEncryptionData(keySize, salt, passwordVerifyValue,
                                                                                      password);
                    entryHeader.CompressedSize -= (uint) (salt.Length + 2);

#endif
                }
            }
            if (entryHeader.IsDirectory)
            {
                return;
            }
            //if (FlagUtility.HasFlag(entryHeader.Flags, HeaderFlags.UsePostDataDescriptor))
            //{
            //    entryHeader.PackedStream = new ReadOnlySubStream(stream);
            //}
            //else
            //{
            switch (mode)
            {
                case StreamingMode.Seekable:
                    {
                        entryHeader.DataStartPosition = stream.Position;
                        stream.Position += entryHeader.CompressedSize;
                    }
                    break;
                case StreamingMode.Streaming:
                    {
                        entryHeader.PackedStream = stream;
                    }
                    break;
                default:
                    {
                        throw new InvalidFormatException("Invalid StreamingMode");
                    }
            }
            //}
        }
    }
}