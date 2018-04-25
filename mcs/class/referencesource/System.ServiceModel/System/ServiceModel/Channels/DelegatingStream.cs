//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.IO;

    // Base Stream that delegates all its methods to another Stream.
    abstract class DelegatingStream : Stream
    {
        Stream stream;

        protected DelegatingStream(Stream stream)
        {
            if (stream == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
            }

            this.stream = stream;
        }

        protected Stream BaseStream
        {
            get
            {
                return stream;
            }
        }

        public override bool CanRead
        {
            get
            {
                return stream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return stream.CanSeek;
            }
        }

        public override bool CanTimeout
        {
            get
            {
                return stream.CanTimeout;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return stream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                return stream.Length;
            }
        }

        public override long Position
        {
            get
            {
                return stream.Position;
            }
            set
            {
                stream.Position = value;
            }
        }

        public override int ReadTimeout
        {
            get
            {
                return stream.ReadTimeout;
            }
            set
            {
                stream.ReadTimeout = value;
            }
        }

        public override int WriteTimeout
        {
            get
            {
                return stream.WriteTimeout;
            }
            set
            {
                stream.WriteTimeout = value;
            }
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return stream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return stream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void Close()
        {
            stream.Close();
        }

        public override int EndRead(IAsyncResult result)
        {
            return stream.EndRead(result);
        }

        public override void EndWrite(IAsyncResult result)
        {
            stream.EndWrite(result);
        }

        public override void Flush()
        {
            stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return stream.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            return stream.ReadByte();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            stream.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            stream.WriteByte(value);
        }
    }
}
