//-----------------------------------------------------------------------------
// <copyright file="DelegatedStream.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------

namespace System.Net
{
    using System;
    using System.Net.Sockets;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    internal class DelegatedStream : Stream
    {
        Stream stream;
        NetworkStream netStream;

        protected DelegatedStream() {
        }
        protected DelegatedStream(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            this.stream = stream;
            netStream = stream as NetworkStream;
        }

        protected Stream BaseStream
        {
            get
            {
                return this.stream;
            }
        }

        public override bool CanRead
        {
            get
            {
                return this.stream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return this.stream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return this.stream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                if (!CanSeek)
                    throw new NotSupportedException(SR.GetString(SR.SeekNotSupported));

                return this.stream.Length;
            }
        }

        public override long Position
        {
            get
            {
                if (!CanSeek)
                    throw new NotSupportedException(SR.GetString(SR.SeekNotSupported));

                return this.stream.Position;
            }
            set
            {
                if (!CanSeek)
                    throw new NotSupportedException(SR.GetString(SR.SeekNotSupported));

                this.stream.Position = value;
            }
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            if (!CanRead)
                throw new NotSupportedException(SR.GetString(SR.ReadNotSupported));

            IAsyncResult result = null;
            
            if(netStream != null){
                result = this.netStream.UnsafeBeginRead (buffer, offset, count, callback, state);
            }
            else{
                result = this.stream.BeginRead (buffer, offset, count, callback, state);
            }
            return result;
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            if (!CanWrite)
                throw new NotSupportedException(SR.GetString(SR.WriteNotSupported));
            
            IAsyncResult result = null;

            if(netStream != null){
                result = this.netStream.UnsafeBeginWrite(buffer, offset, count, callback, state);
            }
            else{
                result = this.stream.BeginWrite (buffer, offset, count, callback, state);
            }
            return result;
        }

        //This calls close on the inner stream
        //however, the stream may not be actually closed, but simpy flushed
        public override void Close()
        {
            this.stream.Close();
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            if (!CanRead)
                throw new NotSupportedException(SR.GetString(SR.ReadNotSupported));

            int read = this.stream.EndRead (asyncResult);
            return read;
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            if (!CanWrite)
                throw new NotSupportedException(SR.GetString(SR.WriteNotSupported));

            this.stream.EndWrite (asyncResult);
        }

        public override void Flush()
        {
            this.stream.Flush();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return this.stream.FlushAsync(cancellationToken);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!CanRead)
                throw new NotSupportedException(SR.GetString(SR.ReadNotSupported));

            int read = this.stream.Read(buffer, offset, count);
            return read;
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (!CanRead)
                throw new NotSupportedException(SR.GetString(SR.ReadNotSupported));

            return this.stream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!CanSeek)
                throw new NotSupportedException(SR.GetString(SR.SeekNotSupported));

            long position = this.stream.Seek(offset, origin);
            return position;
        }

        public override void SetLength(long value)
        {
            if (!CanSeek)
                throw new NotSupportedException(SR.GetString(SR.SeekNotSupported));

            this.stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!CanWrite)
                throw new NotSupportedException(SR.GetString(SR.WriteNotSupported));

            this.stream.Write(buffer, offset, count);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (!CanWrite)
                throw new NotSupportedException(SR.GetString(SR.WriteNotSupported));

            return this.stream.WriteAsync(buffer, offset, count, cancellationToken);
        }
    }
}
