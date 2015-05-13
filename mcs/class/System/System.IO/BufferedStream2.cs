namespace System.IO
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;

    [HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
    internal abstract class BufferedStream2 : Stream
    {
        private byte[] _buffer;
        private int _pendingBufferCopy;
        private int _readLen;
        private int _readPos;
        private int _writePos;
        protected int bufferSize;
        protected internal const int DefaultBufferSize = 0x8000;
        protected long pos;

        protected BufferedStream2()
        {
        }

        protected long AddUnderlyingStreamPosition(long posDelta)
        {
            return Interlocked.Add(ref this.pos, posDelta);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected internal void DiscardBuffer()
        {
            this._readPos = 0;
            this._readLen = 0;
            this._writePos = 0;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (this._writePos > 0)
                {
                    this.FlushWrite(disposing);
                }
            }
            finally
            {
                this._readPos = 0;
                this._readLen = 0;
                this._writePos = 0;
                base.Dispose(disposing);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override void Flush()
        {
            try
            {
                if (this._writePos > 0)
                {
                    this.FlushWrite(false);
                }
                else if (this._readPos < this._readLen)
                {
                    this.FlushRead();
                }
            }
            finally
            {
                this._writePos = 0;
                this._readPos = 0;
                this._readLen = 0;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected void FlushRead()
        {
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected void FlushWrite(bool blockForWrite)
        {
            if (this._writePos > 0)
            {
                this.WriteCore(this._buffer, 0, this._writePos, blockForWrite);
            }
            this._writePos = 0;
        }

        public override void Write(byte[] array, int offset, int count)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array", System.SR.GetString("ArgumentNull_Buffer"));
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", System.SR.GetString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", System.SR.GetString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((array.Length - offset) < count)
            {
                throw new ArgumentException(System.SR.GetString("Argument_InvalidOffLen"));
            }
            if (this._writePos == 0)
            {
                if (!this.CanWrite)
                {
                    System.IO.__Error.WriteNotSupported();
                }
                if (this._readPos < this._readLen)
                {
                    this.FlushRead();
                }
                this._readPos = 0;
                this._readLen = 0;
            }
            if (count == 0)
            {
                return;
            }
        Label_009D:
            while (this._writePos > this.bufferSize)
            {
                Thread.Sleep(1);
            }
            if ((this._writePos == 0) && (count >= this.bufferSize))
            {
                this.WriteCore(array, offset, count, true);
            }
            else
            {
                Thread.BeginCriticalRegion();
                Interlocked.Increment(ref this._pendingBufferCopy);
                int num = Interlocked.Add(ref this._writePos, count);
                int num2 = num - count;
                if (num > this.bufferSize)
                {
                    Interlocked.Decrement(ref this._pendingBufferCopy);
                    Thread.EndCriticalRegion();
                    if (((this._writePos > this.bufferSize) && (num2 <= this.bufferSize)) && (num2 > 0))
                    {
                        while (this._pendingBufferCopy != 0)
                        {
                            Thread.SpinWait(1);
                        }
                        this.WriteCore(this._buffer, 0, num2, true);
                        this._writePos = 0;
                    }
                    goto Label_009D;
                }
                if (this._buffer == null)
                {
                    Interlocked.CompareExchange<byte[]>(ref this._buffer, new byte[this.bufferSize], null);
                }
                Buffer.BlockCopy(array, offset, this._buffer, num2, count);
                Interlocked.Decrement(ref this._pendingBufferCopy);
                Thread.EndCriticalRegion();
            }
        }

        private void WriteCore(byte[] buffer, int offset, int count, bool blockForWrite)
        {
            long num;
            this.WriteCore(buffer, offset, count, blockForWrite, out num);
        }

        protected abstract void WriteCore(byte[] buffer, int offset, int count, bool blockForWrite, out long streamPos);

        protected long UnderlyingStreamPosition
        {
            get
            {
                return this.pos;
            }
            set
            {
                Interlocked.Exchange(ref this.pos, value);
            }
        }
    }
}

