using System.IO;
using SharpCompress.Common;

namespace SharpCompress.IO
{
    internal class ListeningStream : Stream
    {
        private long currentEntryTotalReadBytes;
        private IExtractionListener listener;

        public ListeningStream(IExtractionListener listener, Stream stream)
        {
            Stream = stream;
            this.listener = listener;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stream.Dispose();
            }
        }

        public Stream Stream { get; private set; }

        public override bool CanRead
        {
            get { return Stream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return Stream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return Stream.CanWrite; }
        }

        public override void Flush()
        {
            Stream.Flush();
        }

        public override long Length
        {
            get { return Stream.Length; }
        }

        public override long Position
        {
            get { return Stream.Position; }
            set { Stream.Position = value; }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = Stream.Read(buffer, offset, count);
            currentEntryTotalReadBytes += read;
            listener.FireCompressedBytesRead(currentEntryTotalReadBytes, currentEntryTotalReadBytes);
            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return Stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            Stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Stream.Write(buffer, offset, count);
        }
    }
}