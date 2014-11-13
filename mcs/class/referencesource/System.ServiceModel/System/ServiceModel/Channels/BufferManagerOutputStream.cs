//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel; // for QuotaExceededException
    using System.ServiceModel.Diagnostics.Application;

    class BufferManagerOutputStream : BufferedOutputStream
    {
        string quotaExceededString;

        public BufferManagerOutputStream(string quotaExceededString)
            : base()
        {
            this.quotaExceededString = quotaExceededString;
        }

        public BufferManagerOutputStream(string quotaExceededString, int maxSize)
            : base(maxSize)
        {
            this.quotaExceededString = quotaExceededString;
        }

        public BufferManagerOutputStream(string quotaExceededString, int initialSize, int maxSize, BufferManager bufferManager)
            : base(initialSize, maxSize, BufferManager.GetInternalBufferManager(bufferManager))
        {
            this.quotaExceededString = quotaExceededString;
        }

        public void Init(int initialSize, int maxSizeQuota, BufferManager bufferManager)
        {
            Init(initialSize, maxSizeQuota, maxSizeQuota, bufferManager);
        }

        public void Init(int initialSize, int maxSizeQuota, int effectiveMaxSize, BufferManager bufferManager)
        {
            base.Reinitialize(initialSize, maxSizeQuota, effectiveMaxSize, BufferManager.GetInternalBufferManager(bufferManager));
        }

        protected override Exception CreateQuotaExceededException(int maxSizeQuota)
        {
            string excMsg = SR.GetString(this.quotaExceededString, maxSizeQuota);
            if (TD.MaxSentMessageSizeExceededIsEnabled())
            {
                TD.MaxSentMessageSizeExceeded(excMsg);
            }
            return new QuotaExceededException(excMsg);
        }
    }
}
