using System.Collections.Generic;
using System.IO;
using SharpCompress.Common;
using SharpCompress.Common.Zip;
using SharpCompress.Common.Zip.Headers;

namespace SharpCompress.Reader.Zip
{
    internal class ZipReader : AbstractReader<ZipEntry, ZipVolume>
    {
        private readonly StreamingZipHeaderFactory headerFactory;
        private readonly ZipVolume volume;

        internal ZipReader(Stream stream, Options options, string password)
            : base(options, ArchiveType.Zip)
        {
            volume = new ZipVolume(stream, options);
            headerFactory = new StreamingZipHeaderFactory(password);
        }

        public override ZipVolume Volume
        {
            get { return volume; }
        }

        #region Open

        /// <summary>
        /// Opens a ZipReader for Non-seeking usage with a single volume
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="options"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static ZipReader Open(Stream stream, string password = null,
                                     Options options = Options.KeepStreamsOpen)
        {
            stream.CheckNotNull("stream");
            return new ZipReader(stream, options, password);
        }

        #endregion

        internal override IEnumerable<ZipEntry> GetEntries(Stream stream)
        {
            foreach (ZipHeader h in headerFactory.ReadStreamHeader(stream))
            {
                if (h != null)
                {
                    switch (h.ZipHeaderType)
                    {
                        case ZipHeaderType.LocalEntry:
                            {
                                yield return new ZipEntry(new StreamingZipFilePart(h as LocalEntryHeader,
                                                                                   stream));
                            }
                            break;
                        case ZipHeaderType.DirectoryEnd:
                            {
                                yield break;
                            }
                    }
                }
            }
        }
    }
}