using System.IO;

namespace SharpCompress.IO
{
    internal class ReadOnlySubStream : Stream
    {
        public ReadOnlySubStream(Stream stream, long bytesToRead)
            : this(stream, null, bytesToRead)
        {
        }

        public ReadOnlySubStream(Stream stream, long? origin, long bytesToRead)
        {
            Stream = stream;
            if (origin != null)
            {
                stream.Position = origin.Value;
            }
            BytesLeftToRead = bytesToRead;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //Stream.Dispose();
            }
        }

        private long BytesLeftToRead { get; set; }

        public Stream Stream { get; private set; }

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
            if (BytesLeftToRead < count)
            {
                count = (int)BytesLeftToRead;
            }
            int read = Stream.Read(buffer, offset, count);
            if (read > 0)
            {
                BytesLeftToRead -= read;
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