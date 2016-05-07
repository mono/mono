//------------------------------------------------------------------------------
// <copyright file="BufferedResponseStream.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System;
    using System.IO;
    using System.Web.Services;

    internal class BufferedResponseStream : Stream {
        Stream outputStream;
        byte[] buffer;
        int position;
        bool flushEnabled = true;

        internal BufferedResponseStream(Stream outputStream, int buffersize) {
            buffer = new byte[buffersize];
            this.outputStream = outputStream;
        }

        public override bool CanRead { get { return false; } }

        public override bool CanSeek { get { return false; } }

        public override bool CanWrite { get { return true; } }

        public override long Length { get { throw new NotSupportedException(Res.GetString(Res.StreamDoesNotSeek)); } }

        public override long Position { 
            get { throw new NotSupportedException(Res.GetString(Res.StreamDoesNotSeek)); } 
            set { throw new NotSupportedException(Res.GetString(Res.StreamDoesNotSeek)); } 
        }

        protected override void Dispose(bool disposing) {
            try {
                if (disposing)
                    outputStream.Close();
            }
            finally {
                base.Dispose(disposing);
            }
        }

        internal bool FlushEnabled {
            set { flushEnabled = value; }
        }

        public override void Flush() {
            if (!flushEnabled)
                return; 
            FlushWrite();
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, Object state) {
            throw new NotSupportedException(Res.GetString(Res.StreamDoesNotRead));
        }

        public override int EndRead(IAsyncResult asyncResult) { throw new NotSupportedException(Res.GetString(Res.StreamDoesNotRead)); }

        public override long Seek(long offset, SeekOrigin origin) { throw new NotSupportedException(Res.GetString(Res.StreamDoesNotSeek)); }

        public override void SetLength(long value) { throw new NotSupportedException(Res.GetString(Res.StreamDoesNotSeek)); }

        public override int Read(byte[] buffer, int offset, int count) { throw new NotSupportedException(Res.GetString(Res.StreamDoesNotRead)); }


        public override int ReadByte() { throw new NotSupportedException(Res.GetString(Res.StreamDoesNotRead)); }

        public override void Write(byte[] array, int offset, int count) {
            if (position > 0) {
                int numBytes = buffer.Length - position;   // space left in buffer
                if (numBytes > 0) {
                    if (numBytes > count)
                        numBytes = count;
                    Array.Copy(array, offset, buffer, position, numBytes);
                    position += numBytes;
                    if (count == numBytes) return;
                    offset += numBytes;
                    count -= numBytes;
                }
                FlushWrite();
            }
            // Skip buffer if we have more bytes then will fit in the buffer.
            if (count >= buffer.Length) {
                outputStream.Write(array, offset, count);
                return;
            }

            // Copy remaining bytes into buffer, to write at a later date.
            Array.Copy(array, offset, buffer, position, count);
            position = count;
        }

        private void FlushWrite() {
            if (position > 0) {
                outputStream.Write(buffer, 0, position);
                position = 0;
            }
            outputStream.Flush();
        }

        public override void WriteByte(byte value) {
            if (position == buffer.Length)
                FlushWrite();

            buffer[position++] = value;
        }
    }
}
