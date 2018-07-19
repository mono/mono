//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.Runtime;

    class ByteStreamBufferedMessageData
    {
        ArraySegment<byte> buffer;
        BufferManager bufferManager;
        int refCount;

        public ByteStreamBufferedMessageData(ArraySegment<byte> buffer)
            : this(buffer, null)
        {
        }

        public ByteStreamBufferedMessageData(ArraySegment<byte> buffer, BufferManager bufferManager)
        {
            if (buffer.Array == null)
            {
                throw FxTrace.Exception.ArgumentNull(SR.ArgumentPropertyShouldNotBeNullError("buffer.Array"));
            }

            this.buffer = buffer;
            this.bufferManager = bufferManager;
            this.refCount = 0;
        }

        bool IsClosed
        {
            get
            {
                return this.refCount < 0;
            }
        }

        public ArraySegment<byte> Buffer
        {
            get
            {
                ThrowIfClosed();
                return this.buffer;
            }
        }

        public void Open()
        {
            ThrowIfClosed();
            this.refCount++;
        }

        public void Close()
        {
            if (!this.IsClosed)
            {
                if (--this.refCount <= 0)
                {
                    if (this.bufferManager != null && this.buffer.Array != null)
                    {
                        this.bufferManager.ReturnBuffer(this.buffer.Array);
                    }
                    this.bufferManager = null;
                    this.buffer = default(ArraySegment<byte>);
                    this.refCount = int.MinValue;
                }
            }
        }

        public Stream ToStream()
        {
            return new ByteStreamBufferedMessageDataStream(this);
        }

        void ThrowIfClosed()
        {
            if (this.IsClosed)
            {
                throw FxTrace.Exception.ObjectDisposed(SR.ObjectDisposed(this));
            }
        }

        class ByteStreamBufferedMessageDataStream : MemoryStream
        {
            ByteStreamBufferedMessageData byteStreamBufferedMessageData;

            public ByteStreamBufferedMessageDataStream(ByteStreamBufferedMessageData byteStreamBufferedMessageData)
                : base(byteStreamBufferedMessageData.Buffer.Array, byteStreamBufferedMessageData.Buffer.Offset, byteStreamBufferedMessageData.Buffer.Count, false)
            {
                this.byteStreamBufferedMessageData = byteStreamBufferedMessageData;
                this.byteStreamBufferedMessageData.Open(); //increment the refCount
            }

            public override void Close()
            {
                this.byteStreamBufferedMessageData.Close();
                base.Close();
            }
        }
    }
}
