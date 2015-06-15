using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SharpCompress.Common;
using SharpCompress.Common.Zip;
using SharpCompress.Common.Zip.Headers;
using SharpCompress.Compressor.Deflate;
using SharpCompress.Reader;
using SharpCompress.Reader.Zip;
using SharpCompress.Writer.Zip;

namespace SharpCompress.Archive.Zip
{
    internal class ZipArchive : AbstractWritableArchive<ZipArchiveEntry, ZipVolume>
    {
        private readonly SeekableZipHeaderFactory headerFactory;

        /// <summary>
        /// Gets or sets the compression level applied to files added to the archive,
        /// if the compression method is set to deflate
        /// </summary>
        public CompressionLevel DeflateCompressionLevel { get; set; }

#if !PORTABLE && !NETFX_CORE
        /// <summary>
        /// Constructor expects a filepath to an existing file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="password"></param>
        public static ZipArchive Open(string filePath, string password = null)
        {
            return Open(filePath, Options.None, password);
        }

        /// <summary>
        /// Constructor with a FileInfo object to an existing file.
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="password"></param>
        public static ZipArchive Open(FileInfo fileInfo, string password = null)
        {
            return Open(fileInfo, Options.None, password);
        }

        /// <summary>
        /// Constructor expects a filepath to an existing file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="options"></param>
        /// <param name="password"></param>
        public static ZipArchive Open(string filePath, Options options, string password = null)
        {
            filePath.CheckNotNullOrEmpty("filePath");
            return Open(new FileInfo(filePath), options, password);
        }

        /// <summary>
        /// Constructor with a FileInfo object to an existing file.
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="options"></param>
        /// <param name="password"></param>
        public static ZipArchive Open(FileInfo fileInfo, Options options, string password = null)
        {
            fileInfo.CheckNotNull("fileInfo");
            return new ZipArchive(fileInfo, options, password);
        }
#endif

        /// <summary>
        /// Takes a seekable Stream as a source
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="password"></param>
        public static ZipArchive Open(Stream stream, string password = null)
        {
            stream.CheckNotNull("stream");
            return Open(stream, Options.None, password);
        }

        /// <summary>
        /// Takes a seekable Stream as a source
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="options"></param>
        /// <param name="password"></param>
        public static ZipArchive Open(Stream stream, Options options, string password = null)
        {
            stream.CheckNotNull("stream");
            return new ZipArchive(stream, options, password);
        }

#if !PORTABLE && !NETFX_CORE
        public static bool IsZipFile(string filePath, string password = null)
        {
            return IsZipFile(new FileInfo(filePath), password);
        }

        public static bool IsZipFile(FileInfo fileInfo, string password = null)
        {
            if (!fileInfo.Exists)
            {
                return false;
            }
            using (Stream stream = fileInfo.OpenRead())
            {
                return IsZipFile(stream, password);
            }
        }
#endif

        public static bool IsZipFile(Stream stream, string password = null)
        {
            StreamingZipHeaderFactory headerFactory = new StreamingZipHeaderFactory(password);
            try
            {
                ZipHeader header =
                    headerFactory.ReadStreamHeader(stream).FirstOrDefault(x => x.ZipHeaderType != ZipHeaderType.Split);
                if (header == null)
                {
                    return false;
                }
                return Enum.IsDefined(typeof (ZipHeaderType), header.ZipHeaderType);
            }
            catch (CryptographicException)
            {
                return true;
            }
            catch
            {
                return false;
            }
        }

#if !PORTABLE && !NETFX_CORE
        /// <summary>
        /// Constructor with a FileInfo object to an existing file.
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="options"></param>
        /// <param name="password"></param>
        internal ZipArchive(FileInfo fileInfo, Options options, string password = null)
            : base(ArchiveType.Zip, fileInfo, options)
        {
            headerFactory = new SeekableZipHeaderFactory(password);
        }

        protected override IEnumerable<ZipVolume> LoadVolumes(FileInfo file, Options options)
        {
            if (FlagUtility.HasFlag(options, Options.KeepStreamsOpen))
            {
                options = (Options)FlagUtility.SetFlag(options, Options.KeepStreamsOpen, false);
            }
            return new ZipVolume(file.OpenRead(), options).AsEnumerable();
        }
#endif

        internal ZipArchive()
            : base(ArchiveType.Zip)
        {
        }

        /// <summary>
        /// Takes multiple seekable Streams for a multi-part archive
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="options"></param>
        /// <param name="password"></param>
        internal ZipArchive(Stream stream, Options options, string password = null)
            : base(ArchiveType.Zip, stream, options)
        {
            headerFactory = new SeekableZipHeaderFactory(password);
        }

        protected override IEnumerable<ZipVolume> LoadVolumes(IEnumerable<Stream> streams, Options options)
        {
            return new ZipVolume(streams.First(), options).AsEnumerable();
        }

        protected override IEnumerable<ZipArchiveEntry> LoadEntries(IEnumerable<ZipVolume> volumes)
        {
            var volume = volumes.Single();
            Stream stream = volume.Stream;
            foreach (ZipHeader h in headerFactory.ReadSeekableHeader(stream))
            {
                if (h != null)
                {
                    switch (h.ZipHeaderType)
                    {
                        case ZipHeaderType.DirectoryEntry:
                            {
                                yield return new ZipArchiveEntry(this,
                                                                 new SeekableZipFilePart(headerFactory,
                                                                                         h as DirectoryEntryHeader,
                                                                                         stream));
                            }
                            break;
                        case ZipHeaderType.DirectoryEnd:
                            {
                                byte[] bytes = (h as DirectoryEndHeader).Comment;
                                volume.Comment = ArchiveEncoding.Default.GetString(bytes, 0, bytes.Length);
                                yield break;
                            }
                    }
                }
            }
        }     

        protected override void SaveTo(Stream stream, CompressionInfo compressionInfo, Encoding encoding,
                                       IEnumerable<ZipArchiveEntry> oldEntries,
                                       IEnumerable<ZipArchiveEntry> newEntries)
        {
            using (var writer = new ZipWriter(stream, compressionInfo, string.Empty, encoding))
            {
                foreach (var entry in oldEntries.Concat(newEntries)
                                                .Where(x => !x.IsDirectory))
                {
                    using (var entryStream = entry.OpenEntryStream())
                    {
                        writer.Write(entry.Key, entryStream, entry.LastModifiedTime, string.Empty);
                    }
                }
            }
        }

        protected override ZipArchiveEntry CreateEntryInternal(string filePath, Stream source, long size, DateTime? modified,
                                                       bool closeStream)
        {
            return new ZipWritableArchiveEntry(this, source, filePath, size, modified, closeStream);
        }

        public static ZipArchive Create()
        {
            return new ZipArchive();
        }

        protected override IReader CreateReaderForSolidExtraction()
        {
            var stream = Volumes.Single().Stream;
            stream.Position = 0;
            return ZipReader.Open(stream);
        }
    }
}