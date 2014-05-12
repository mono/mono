using System;
using System.IO;

namespace SharpCompress.Common
{
    internal class EntryStream : Stream
    {
        private Stream stream;
        private bool completed;
        private bool isDisposed;

        internal EntryStream(Stream stream)
        {
            this.stream = stream;
        }

        /// <summary>
        /// When reading a stream from OpenEntryStream, the stream must be completed so use this to finish reading the entire entry.
        /// </summary>
        public void SkipEntry()
        {
            var buffer = new byte[4096];
            while (Read(buffer, 0, buffer.Length) > 0)
            {
            }
            completed = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (!completed)
            {
                throw new InvalidOperationException(
                    "EntryStream has not been fully consumed.  Read the entire stream or use SkipEntry.");
            }
            if (isDisposed)
            {
                return;
            }
            isDisposed = true;
            base.Dispose(disposing);
            stream.Dispose();
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            throw new System.NotImplementedException();
        }

        public override long Length
        {
            get { throw new System.NotImplementedException(); }
        }

        public override long Position
        {
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = stream.Read(buffer, offset, count);
            if (read <= 0)
            {
                completed = true;
            }
            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new System.NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new System.NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }
    }
}