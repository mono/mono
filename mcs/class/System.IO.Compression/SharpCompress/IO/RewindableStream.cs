using System.IO;

namespace SharpCompress.IO
{
    internal class RewindableStream : Stream
    {
        private readonly Stream stream;
        private MemoryStream bufferStream = new MemoryStream();
        private bool isRewound;
        private bool isDisposed;

        public RewindableStream(Stream stream)
        {
            this.stream = stream;
        }

        internal bool IsRecording { get; private set; }

        protected override void Dispose(bool disposing)
        {
            if (isDisposed)
            {
                return;
            }
            isDisposed = true;
            base.Dispose(disposing);
            if (disposing)
            {
                stream.Dispose();
            }
        }

        public void Rewind(bool stopRecording)
        {
            isRewound = true;
            IsRecording = !stopRecording;
            bufferStream.Position = 0;
        }

        public void Rewind(MemoryStream buffer)
        {
            if (bufferStream.Position >= buffer.Length)
            {
                bufferStream.Position -= buffer.Length;
            }
            else
            {
                bufferStream.TransferTo(buffer);
                bufferStream = buffer;
                bufferStream.Position = 0;
            }
            isRewound = true;
        }

        public void StartRecording()
        {
            //if (isRewound && bufferStream.Position != 0)
            //   throw new System.NotImplementedException();
            if (bufferStream.Position != 0)
            {
                byte[] data = bufferStream.ToArray();
                long position = bufferStream.Position;
                bufferStream.SetLength(0);
                bufferStream.Write(data, (int)position, data.Length - (int)position);
                bufferStream.Position = 0;
            }
            IsRecording = true;
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
            get { return stream.Position + bufferStream.Position - bufferStream.Length; }
            set
            {
                if (!isRewound)
                {
                    stream.Position = value;
                }
                else if (value < stream.Position - bufferStream.Length || value >= stream.Position)
                {
                    stream.Position = value;
                    isRewound = false;
                    bufferStream.SetLength(0);
                }
                else
                {
                    bufferStream.Position = value - stream.Position + bufferStream.Length;
                }
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read;
            if (isRewound && bufferStream.Position != bufferStream.Length)
            {
                read = bufferStream.Read(buffer, offset, count);
                if (read < count)
                {
                    int tempRead = stream.Read(buffer, offset + read, count - read);
                    if (IsRecording)
                    {
                        bufferStream.Write(buffer, offset + read, tempRead);
                    }
                    read += tempRead;
                }
                if (bufferStream.Position == bufferStream.Length && !IsRecording)
                {
                    isRewound = false;
                    bufferStream.SetLength(0);
                }
                return read;
            }

            read = stream.Read(buffer, offset, count);
            if (IsRecording)
            {
                bufferStream.Write(buffer, offset, read);
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