using System.IO;
using SharpCompress.IO;

namespace SharpCompress.Common
{
    internal abstract class Volume : IVolume
    {
        private readonly Stream actualStream;

        internal Volume(Stream stream, Options options)
        {
            actualStream = stream;
            Options = options;
        }

        internal Stream Stream
        {
            get { return new NonDisposingStream(actualStream); }
        }

        internal Options Options { get; private set; }

        /// <summary>
        /// RarArchive is the first volume of a multi-part archive.
        /// Only Rar 3.0 format and higher
        /// </summary>
        public virtual bool IsFirstVolume
        {
            get { return true; }
        }

        /// <summary>
        /// RarArchive is part of a multi-part archive.
        /// </summary>
        public virtual bool IsMultiVolume
        {
            get { return true; }
        }

        private bool disposed;

        public void Dispose()
        {
            if (!Options.HasFlag(Options.KeepStreamsOpen) && !disposed)
            {
                actualStream.Dispose();
                disposed = true;
            }
        }
    }
}