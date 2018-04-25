//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Workflow.Runtime
{
    using System.Messaging;
    using System.Runtime.Serialization;

    [Serializable]
    class QueueException : InvalidOperationException
    {
        [NonSerialized]
        MessageQueueErrorCode errorCode;

        public QueueException(string message, MessageQueueErrorCode errorCode) : base(message)
        {
            this.errorCode = errorCode;
        }

        protected QueueException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        public MessageQueueErrorCode ErrorCode
        {
            get
            {
                return this.errorCode;
            }
        }
    }
}
