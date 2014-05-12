using System;
using System.IO;
using SharpCompress.Common;

namespace SharpCompress.Writer
{
    internal abstract class AbstractWriter : IWriter
    {
        private bool closeStream;
        private bool isDisposed;

        protected AbstractWriter(ArchiveType type)
        {
            this.WriterType = type;
        }

        protected void InitalizeStream(Stream stream, bool closeStream)
        {
            this.OutputStream = stream;
            this.closeStream = closeStream;
        }

        protected Stream OutputStream { get; private set; }

        public ArchiveType WriterType { get; private set; }

        public abstract void Write(string filename, System.IO.Stream source, DateTime? modificationTime);

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing && closeStream)
            {
                OutputStream.Dispose();
            }
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                GC.SuppressFinalize(this);
                Dispose(true);
                isDisposed = true;
            }
        }

        ~AbstractWriter()
        {
            if (!isDisposed)
            {
                Dispose(false);
                isDisposed = true;
            }
        }
    }
}