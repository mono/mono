using System;
using System.IO;
#if GZIP
using SharpCompress.Archive.GZip;
#endif
#if RAR
using SharpCompress.Archive.Rar;
#endif
#if TAR
using SharpCompress.Archive.Tar;
#endif
using SharpCompress.Archive.Zip;
using SharpCompress.Common;
using SharpCompress.Compressor;
#if BZIP2
using SharpCompress.Compressor.BZip2;
#endif
using SharpCompress.IO;
#if GZIP
using SharpCompress.Reader.GZip;
#endif
#if RAR
using SharpCompress.Reader.Rar;
#endif
#if TAR
using SharpCompress.Reader.Tar;
#endif
using SharpCompress.Reader.Zip;
#if DEFLATE
using GZipStream = SharpCompress.Compressor.Deflate.GZipStream;
#endif

namespace SharpCompress.Reader
{
    internal static class ReaderFactory
    {
        /// <summary>
        /// Opens a Reader for Non-seeking usage
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IReader Open(Stream stream, Options options = Options.KeepStreamsOpen)
        {
            stream.CheckNotNull("stream");

            RewindableStream rewindableStream = new RewindableStream(stream);
            rewindableStream.StartRecording();
            if (ZipArchive.IsZipFile(rewindableStream, null))
            {
                rewindableStream.Rewind(true);
                return ZipReader.Open(rewindableStream, null, options);
            }
#if GZIP
            rewindableStream.Rewind(false);
            if (GZipArchive.IsGZipFile(rewindableStream))
            {
                rewindableStream.Rewind(false);
                GZipStream testStream = new GZipStream(rewindableStream, CompressionMode.Decompress);
                if (TarArchive.IsTarFile(testStream))
                {
                    rewindableStream.Rewind(true);
                    return new TarReader(rewindableStream, CompressionType.GZip, options);
                }
                rewindableStream.Rewind(true);
                return GZipReader.Open(rewindableStream, options);
            }
#endif

#if BZIP2
            rewindableStream.Rewind(false);
            if (BZip2Stream.IsBZip2(rewindableStream))
            {
                rewindableStream.Rewind(false);
                BZip2Stream testStream = new BZip2Stream(rewindableStream, CompressionMode.Decompress, false);
#if TAR
                if (TarArchive.IsTarFile(testStream))
                {
                    rewindableStream.Rewind(true);
                    return new TarReader(rewindableStream, CompressionType.BZip2, options);
                }
#endif
            }
#endif

#if TAR
            rewindableStream.Rewind(false);
            if (TarArchive.IsTarFile(rewindableStream))
            {
                rewindableStream.Rewind(true);
                return TarReader.Open(rewindableStream, options);
            }
#endif
#if RAR
            rewindableStream.Rewind(false);
            if (RarArchive.IsRarFile(rewindableStream, options))
            {
                rewindableStream.Rewind(true);
                return RarReader.Open(rewindableStream, options);
            }
#endif

            throw new InvalidOperationException("Cannot determine compressed stream type.  Supported Reader Formats: Zip, GZip, BZip2, Tar, Rar");
        }
    }
}