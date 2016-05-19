//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;

    public abstract class BufferManager
    {
        public abstract byte[] TakeBuffer(int bufferSize);
        public abstract void ReturnBuffer(byte[] buffer);
        public abstract void Clear();

        public static BufferManager CreateBufferManager(long maxBufferPoolSize, int maxBufferSize)
        {
            if (maxBufferPoolSize < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("maxBufferPoolSize",
                    maxBufferPoolSize, SR.GetString(SR.ValueMustBeNonNegative)));
            }

            if (maxBufferSize < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("maxBufferSize",
                    maxBufferSize, SR.GetString(SR.ValueMustBeNonNegative)));
            }

            return new WrappingBufferManager(InternalBufferManager.Create(maxBufferPoolSize, maxBufferSize));
        }

        internal static InternalBufferManager GetInternalBufferManager(BufferManager bufferManager)
        {
            if (bufferManager is WrappingBufferManager)
            {
                return ((WrappingBufferManager)bufferManager).InternalBufferManager;
            }
            else
            {
                return new WrappingInternalBufferManager(bufferManager);
            }
        }

        class WrappingBufferManager : BufferManager
        {
            InternalBufferManager innerBufferManager;

            public WrappingBufferManager(InternalBufferManager innerBufferManager)
            {
                this.innerBufferManager = innerBufferManager;
            }

            public InternalBufferManager InternalBufferManager
            {
                get { return this.innerBufferManager; }
            }

            public override byte[] TakeBuffer(int bufferSize)
            {
                if (bufferSize < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("bufferSize", bufferSize,
                        SR.GetString(SR.ValueMustBeNonNegative)));
                }

                return this.innerBufferManager.TakeBuffer(bufferSize);
            }

            public override void ReturnBuffer(byte[] buffer)
            {
                if (buffer == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");
                }

                this.innerBufferManager.ReturnBuffer(buffer);
            }

            public override void Clear()
            {
                this.innerBufferManager.Clear();
            }
        }

        class WrappingInternalBufferManager : InternalBufferManager
        {
            BufferManager innerBufferManager;

            public WrappingInternalBufferManager(BufferManager innerBufferManager)
            {
                this.innerBufferManager = innerBufferManager;
            }

            public override void Clear()
            {
                this.innerBufferManager.Clear();
            }

            public override void ReturnBuffer(byte[] buffer)
            {
                this.innerBufferManager.ReturnBuffer(buffer);
            }

            public override byte[] TakeBuffer(int bufferSize)
            {
                return this.innerBufferManager.TakeBuffer(bufferSize);
            }
        }
    }
}
