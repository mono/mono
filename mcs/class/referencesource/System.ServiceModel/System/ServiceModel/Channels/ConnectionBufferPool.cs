//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Runtime;

    class ConnectionBufferPool : QueuedObjectPool<byte[]>
    {
        const int SingleBatchSize = 128 * 1024;
        const int MaxBatchCount = 16;
        const int MaxFreeCountFactor = 4;
        int bufferSize;

        public ConnectionBufferPool(int bufferSize)
        {
            int batchCount = ComputeBatchCount(bufferSize);
            this.Initialize(bufferSize, batchCount, batchCount * MaxFreeCountFactor);
        }

        public ConnectionBufferPool(int bufferSize, int maxFreeCount)
        {
            this.Initialize(bufferSize, ComputeBatchCount(bufferSize), maxFreeCount);
        }

        void Initialize(int bufferSize, int batchCount, int maxFreeCount)
        {
            Fx.Assert(bufferSize >= 0, "bufferSize must be non-negative");
            Fx.Assert(batchCount > 0, "batchCount must be positive");
            Fx.Assert(maxFreeCount >= 0, "maxFreeCount must be non-negative");

            this.bufferSize = bufferSize;
            if (maxFreeCount < batchCount)
            {
                maxFreeCount = batchCount;
            }
            base.Initialize(batchCount, maxFreeCount);
        }

        public int BufferSize
        {
            get
            {
                return this.bufferSize;
            }
        }

        protected override byte[] Create()
        {
            return DiagnosticUtility.Utility.AllocateByteArray(this.bufferSize);
        }

        static int ComputeBatchCount(int bufferSize)
        {
            int batchCount;
            if (bufferSize != 0)
            {
                batchCount = (SingleBatchSize + bufferSize - 1) / bufferSize;
                if (batchCount > MaxBatchCount)
                {
                    batchCount = MaxBatchCount;
                }
            }
            else
            {
                // It's OK to have zero bufferSize
                batchCount = MaxBatchCount;
            }
            return batchCount;
        }
    }
}
