//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable]
    public class RetryException : CommunicationException
    {
        public RetryException()
            : this(null, null)
        {
        }

        public RetryException(string message)
            : this(message, null)
        {
        }

        public RetryException(string message, Exception innerException)
            : base(message ?? SR.GetString(SR.RetryGenericMessage), innerException)
        {
        }

        [SecurityCritical]
        protected RetryException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
