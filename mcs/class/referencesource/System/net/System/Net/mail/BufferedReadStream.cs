//-----------------------------------------------------------------------------
// <copyright file="BufferedReadStream.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------

namespace System.Net
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    internal class BufferedReadStream : DelegatedStream
    {
        byte[] storedBuffer;
        int storedLength;
        int storedOffset;
        bool readMore;

        internal BufferedReadStream(Stream stream) : this(stream, false)
        {
        }

        internal BufferedReadStream(Stream stream, bool readMore) : base(stream)
        {
            this.readMore = readMore;
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            ReadAsyncResult result = new ReadAsyncResult(this, callback, state);
            result.Read(buffer, offset, count);
            return result;
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            int read = ReadAsyncResult.End(asyncResult);
            return read;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = 0;
            if (this.storedOffset < this.storedLength)
            {
                read = Math.Min(count, this.storedLength - this.storedOffset);
                Buffer.BlockCopy(this.storedBuffer, this.storedOffset, buffer, offset, read);
                this.storedOffset += read;
                if (read == count || !this.readMore)
                {
                    return read;
                }
                offset += read;
                count -= read;
            }
            return read + base.Read(buffer, offset, count);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            int read = 0;
            if (this.storedOffset >= this.storedLength)
            {
                return base.ReadAsync(buffer, offset, count, cancellationToken);
            }

            read = Math.Min(count, this.storedLength - this.storedOffset);
            Buffer.BlockCopy(this.storedBuffer, this.storedOffset, buffer, offset, read);
            this.storedOffset += read;
            if (read == count || !this.readMore)
            {
                return Task.FromResult<int>(read);
            }
            offset += read;
            count -= read;

            return ReadMoreAsync(read, buffer, offset, count, cancellationToken);
        }

        private async Task<int> ReadMoreAsync(int bytesAlreadyRead, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            int returnValue = await base.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
            return bytesAlreadyRead + returnValue;
        }

        public override int ReadByte()
        {
            if (this.storedOffset < this.storedLength)
            {
                return (int)this.storedBuffer[this.storedOffset++];
            }
            else
            {
                return base.ReadByte();
            }
        }

        
        // adds additional content to the beginning of the buffer
        // so the layout of the storedBuffer will be
        // <buffer><existingBuffer>
        // after calling push
        internal void Push(byte[] buffer, int offset, int count)
        {
            if (count == 0)
                return;

            if (this.storedOffset == this.storedLength)
            {
                if (this.storedBuffer == null || this.storedBuffer.Length < count)
                {
                    this.storedBuffer = new byte[count];
                }
                this.storedOffset = 0;
                this.storedLength = count;
            }
            else
            {
                // if there's room to just insert before existing data
                if (count <= this.storedOffset)
                {
                    this.storedOffset -= count;
                }
                // if there's room in the buffer but need to shift things over
                else if (count <= this.storedBuffer.Length - this.storedLength + this.storedOffset)
                {
                    Buffer.BlockCopy(this.storedBuffer, this.storedOffset, this.storedBuffer, count, this.storedLength - this.storedOffset);
                    this.storedLength += count - this.storedOffset;
                    this.storedOffset = 0;
                }
                else
                {
                    byte[] newBuffer = new byte[count + this.storedLength - this.storedOffset];
                    Buffer.BlockCopy(this.storedBuffer, this.storedOffset, newBuffer, count, this.storedLength - this.storedOffset);
                    this.storedLength += count - this.storedOffset;
                    this.storedOffset = 0;
                    this.storedBuffer = newBuffer;
                }
            }
            Buffer.BlockCopy(buffer, offset, this.storedBuffer, this.storedOffset, count);
        }

        // adds additional content to the end of the buffer
        // so the layout of the storedBuffer will be
        // <existingBuffer><buffer>
        // after calling append
        internal void Append(byte[] buffer, int offset, int count)
        {
            if (count == 0)
                return;

            int newBufferPosition;
            if (this.storedOffset == this.storedLength)
            {
                if (this.storedBuffer == null || this.storedBuffer.Length < count)
                {
                    this.storedBuffer = new byte[count];
                }
                this.storedOffset = 0;
                this.storedLength = count;
                newBufferPosition = 0;
            }
            else
            {
                // if there's room to just insert after existing data
                if (count <= this.storedBuffer.Length - this.storedLength)
                {
                    //no preperation necessary
                    newBufferPosition = this.storedLength;
                    this.storedLength += count;
                }
                // if there's room in the buffer but need to shift things over
                else if (count <= this.storedBuffer.Length - this.storedLength + this.storedOffset)
                {
                    Buffer.BlockCopy(this.storedBuffer, this.storedOffset, this.storedBuffer, 0, this.storedLength - this.storedOffset);
                    newBufferPosition = this.storedLength - this.storedOffset;
                    this.storedOffset = 0;
                    this.storedLength = count + newBufferPosition;
                }
                else
                {
                    // the buffer is too small
                    // allocate new buffer
                    byte[] newBuffer = new byte[count + this.storedLength - this.storedOffset];
                    // and prepopulate the remaining content of the original buffer
                    Buffer.BlockCopy(this.storedBuffer, this.storedOffset, newBuffer, 0, this.storedLength - this.storedOffset);
                    newBufferPosition = this.storedLength - this.storedOffset;
                    this.storedOffset = 0;
                    this.storedLength = count + newBufferPosition;
                    this.storedBuffer = newBuffer;
                }
            }

            Buffer.BlockCopy(buffer, offset, this.storedBuffer, newBufferPosition, count);
        }

        class ReadAsyncResult : LazyAsyncResult
        {
            BufferedReadStream parent;
            int read;
            static AsyncCallback onRead = new AsyncCallback(OnRead);

            internal ReadAsyncResult(BufferedReadStream parent, AsyncCallback callback, object state) : base(null,state,callback)
            {
                this.parent = parent;
            }

            internal void Read(byte[] buffer, int offset, int count){
                if (parent.storedOffset < parent.storedLength)
                {
                    this.read = Math.Min(count, parent.storedLength - parent.storedOffset);
                    Buffer.BlockCopy(parent.storedBuffer, parent.storedOffset, buffer, offset, this.read);
                    parent.storedOffset += this.read;
                    if (this.read == count || !this.parent.readMore)
                    {
                        this.InvokeCallback();
                        return;
                    }
                    count -= this.read;
                    offset += this.read;
                }
                IAsyncResult result = parent.BaseStream.BeginRead(buffer, offset, count, onRead, this);
                if (result.CompletedSynchronously)
                {
                    // 
                    this.read += parent.BaseStream.EndRead(result);
                    InvokeCallback();
                }
            }

            internal static int End(IAsyncResult result)
            {
                ReadAsyncResult thisPtr = (ReadAsyncResult)result;
                thisPtr.InternalWaitForCompletion();
                return thisPtr.read;
            }

            static void OnRead(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ReadAsyncResult thisPtr = (ReadAsyncResult)result.AsyncState;
                    try
                    {
                        thisPtr.read += thisPtr.parent.BaseStream.EndRead(result);
                        thisPtr.InvokeCallback();
                    }
                    catch (Exception e)
                    {
                        if (thisPtr.IsCompleted)
                            throw;
                        thisPtr.InvokeCallback(e);
                    }
                }
            }
        }
    }
}
