//------------------------------------------------------------  
// Copyright (c) Microsoft Corporation.  All rights reserved.   
//------------------------------------------------------------  

namespace System.ServiceModel.Channels
{
    class MsmqInputMessage : NativeMsmqMessage
    {
        BufferProperty senderId;
        IntProperty senderIdLength;
        LongProperty lookupId;
        IntProperty abortCount;
        IntProperty moveCount;
        BufferProperty senderCertificate;
        IntProperty senderCertificateLength;
        IntProperty lastMovedTime;
        BufferProperty body;
        IntProperty bodyLength;
        BufferProperty messageId;
        ShortProperty cls;
        int maxBufferSize;
        const int maxSize = 4 * 1024 * 1024;
        const int initialBodySize = 4096;
        const int initialSenderIdSize = 256;
        const int initialCertificateSize = 4096;

        public MsmqInputMessage()
            : this(0, maxSize)
        {
        }

        public MsmqInputMessage(int maxBufferSize)
            : this(0, maxBufferSize)
        {
        }

        protected MsmqInputMessage(int additionalPropertyCount, int maxBufferSize)
            : this(additionalPropertyCount, new SizeQuota(maxBufferSize))
        {
        }

        protected MsmqInputMessage(int additionalPropertyCount, SizeQuota bufferSizeQuota)
            : base(12 + additionalPropertyCount)
        {
            this.maxBufferSize = bufferSizeQuota.MaxSize;
            this.body = new BufferProperty(this, UnsafeNativeMethods.PROPID_M_BODY,
                bufferSizeQuota.AllocIfAvailable(initialBodySize));
            this.bodyLength = new IntProperty(this, UnsafeNativeMethods.PROPID_M_BODY_SIZE);
            this.messageId = new BufferProperty(this, UnsafeNativeMethods.PROPID_M_MSGID,
                UnsafeNativeMethods.PROPID_M_MSGID_SIZE);
            this.lookupId = new LongProperty(this, UnsafeNativeMethods.PROPID_M_LOOKUPID);
            this.cls = new ShortProperty(this, UnsafeNativeMethods.PROPID_M_CLASS);
            this.senderId = new BufferProperty(this, UnsafeNativeMethods.PROPID_M_SENDERID, initialSenderIdSize);
            this.senderIdLength = new IntProperty(this, UnsafeNativeMethods.PROPID_M_SENDERID_LEN);
            this.senderCertificate = new BufferProperty(this, UnsafeNativeMethods.PROPID_M_SENDER_CERT,
                bufferSizeQuota.AllocIfAvailable(initialCertificateSize));
            this.senderCertificateLength = new IntProperty(this, UnsafeNativeMethods.PROPID_M_SENDER_CERT_LEN);
            if (Msmq.IsAdvancedPoisonHandlingSupported)
            {
                this.lastMovedTime = new IntProperty(this, UnsafeNativeMethods.PROPID_M_LAST_MOVE_TIME);
                this.abortCount = new IntProperty(this, UnsafeNativeMethods.PROPID_M_ABORT_COUNT);
                this.moveCount = new IntProperty(this, UnsafeNativeMethods.PROPID_M_MOVE_COUNT);
            }
        }

        public override void GrowBuffers()
        {
            OnGrowBuffers(new SizeQuota(this.maxBufferSize));
        }

        protected virtual void OnGrowBuffers(SizeQuota bufferSizeQuota)
        {
            bufferSizeQuota.Alloc(this.senderIdLength.Value);
            this.senderId.EnsureBufferLength(this.senderIdLength.Value);

            bufferSizeQuota.Alloc(this.senderCertificateLength.Value);
            this.senderCertificate.EnsureBufferLength(this.senderCertificateLength.Value);

            bufferSizeQuota.Alloc(this.bodyLength.Value);
            this.body.EnsureBufferLength(this.bodyLength.Value);
        }

        public BufferProperty SenderId
        {
            get { return this.senderId; }
        }

        public IntProperty SenderIdLength
        {
            get { return this.senderIdLength; }
        }

        public LongProperty LookupId
        {
            get { return this.lookupId; }
        }

        public IntProperty AbortCount
        {
            get { return this.abortCount; }
        }

        public IntProperty MoveCount
        {
            get { return this.moveCount; }
        }

        public BufferProperty SenderCertificate
        {
            get { return this.senderCertificate; }
        }

        public IntProperty SenderCertificateLength
        {
            get { return this.senderCertificateLength; }
        }

        public IntProperty LastMovedTime
        {
            get { return this.lastMovedTime; }
        }

        public BufferProperty Body
        {
            get { return this.body; }
        }

        public IntProperty BodyLength
        {
            get { return this.bodyLength; }
        }

        public BufferProperty MessageId
        {
            get { return this.messageId; }
        }

        public ShortProperty Class
        {
            get { return this.cls; }
        }

        protected class SizeQuota
        {
            int remainingSize;
            int maxSize;

            public SizeQuota(int maxSize)
            {
                this.maxSize = maxSize;
                this.remainingSize = maxSize;
            }

            public int MaxSize
            {
                get { return this.maxSize; }
            }

            public void Alloc(int requiredSize)
            {
                if (requiredSize > this.remainingSize)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(MaxMessageSizeStream.CreateMaxReceivedMessageSizeExceededException(this.maxSize));
                }
                this.remainingSize -= requiredSize;
            }

            public int AllocIfAvailable(int desiredSize)
            {
                int availableSize = Math.Min(desiredSize, this.remainingSize);
                this.remainingSize -= availableSize;
                return availableSize;
            }
        }
    }
}
