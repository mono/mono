using System;
using System.IO;
using SharpCompress.Common;
#if GZIP
using SharpCompress.Writer.GZip;
#endif
#if TAR
using SharpCompress.Writer.Tar;
#endif
using SharpCompress.Writer.Zip;

namespace SharpCompress.Writer
{
    internal static class WriterFactory
    {
        public static IWriter Open(Stream stream, ArchiveType archiveType, CompressionType compressionType)
        {
            return Open(stream, archiveType, new CompressionInfo
                                                 {
                                                     Type = compressionType
                                                 });
        }

        public static IWriter Open(Stream stream, ArchiveType archiveType, CompressionInfo compressionInfo)
        {
            switch (archiveType)
            {
#if GZIP
                case ArchiveType.GZip:
                    {
                        if (compressionInfo.Type != CompressionType.GZip)
                        {
                            throw new InvalidFormatException("GZip archives only support GZip compression type.");
                        }
                        return new GZipWriter(stream);
                    }
#endif
                case ArchiveType.Zip:
                    {
                        return new ZipWriter(stream, compressionInfo, null);
                    }
#if TAR
                case ArchiveType.Tar:
                    {
                        return new TarWriter(stream, compressionInfo);
                    }
#endif
                default:
                    {
                        throw new NotSupportedException("Archive Type does not have a Writer: " + archiveType);
                    }
            }
        }
    }
}