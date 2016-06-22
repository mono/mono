using System;
using System.Collections.Generic;
using SharpCompress.Common.Zip.Headers;

namespace SharpCompress.Common.Zip
{
    internal class ZipEntry : Entry
    {
        private readonly ZipFilePart filePart;
        private DateTime? lastModifiedTime;

        internal ZipEntry(ZipFilePart filePart)
        {
            if (filePart != null)
            {
                this.filePart = filePart;
                lastModifiedTime = Utility.DosDateToDateTime(filePart.Header.LastModifiedDate,
                                                             filePart.Header.LastModifiedTime);
                if (lastModifiedTime == default(DateTime))
                {
                    // On .NET on Windows, for zip entries that don't have a last write time,
                    // the return value for ZipArchiveEntry.LastWriteTime is:
                    //   1/1/1980 12:00:00 AM, Ticks=624511296000000000
                    lastModifiedTime = new DateTime(624511296000000000);
                }
            }
        }

        public override CompressionType CompressionType
        {
            get
            {
                switch (filePart.Header.CompressionMethod)
                {
                    case ZipCompressionMethod.BZip2:
                        {
                            return CompressionType.BZip2;
                        }
                    case ZipCompressionMethod.Deflate:
                        {
                            return CompressionType.Deflate;
                        }
                    case ZipCompressionMethod.LZMA:
                        {
                            return CompressionType.LZMA;
                        }
                    case ZipCompressionMethod.PPMd:
                        {
                            return CompressionType.PPMd;
                        }
                    case ZipCompressionMethod.None:
                        {
                            return CompressionType.None;
                        }
                    default:
                        {
                            return CompressionType.Unknown;
                        }
                }
            }
        }

        public override uint Crc
        {
            get { return filePart.Header.Crc; }
        }

        public override string Key
        {
            get { return filePart.Header.Name; }
        }

        public override long CompressedSize
        {
            get { return filePart.Header.CompressedSize; }
        }

        public override long Size
        {
            get { return filePart.Header.UncompressedSize; }
        }

        public override DateTime? LastModifiedTime
        {
            get { return lastModifiedTime; }
            set { lastModifiedTime = value; }
        }

        public override DateTime? CreatedTime
        {
            get { return null; }
        }

        public override DateTime? LastAccessedTime
        {
            get { return null; }
        }

        public override DateTime? ArchivedTime
        {
            get { return null; }
        }

        public override bool IsEncrypted
        {
            get { return FlagUtility.HasFlag(filePart.Header.Flags, HeaderFlags.Encrypted); }
        }

        public override bool IsDirectory
        {
            get { return filePart.Header.IsDirectory; }
        }

        public override bool IsSplit
        {
            get { return false; }
        }

        internal override IEnumerable<FilePart> Parts
        {
            get { return filePart.AsEnumerable<FilePart>(); }
        }
    }
}