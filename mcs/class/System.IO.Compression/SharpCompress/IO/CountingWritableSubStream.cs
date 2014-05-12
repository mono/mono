using System;
using System.IO;

namespace SharpCompress.IO
{
    internal class CountingWritableSubStream : Stream
    {
        private Stream writableStream;

        internal CountingWritableSubStream(Stream stream)
        {
            writableStream = stream;
        }

        public uint Count { get; private set; }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush()
        {
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            writableStream.Write(buffer, offset, count);
            Count += (uint) count;
        }
    }
}