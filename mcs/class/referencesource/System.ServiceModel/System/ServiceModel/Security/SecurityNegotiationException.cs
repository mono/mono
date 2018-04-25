//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class SecurityNegotiationException : CommunicationException
    {
        public SecurityNegotiationException()
            : base()
        {
        }

        public SecurityNegotiationException(String message)
            : base(message)
        {
        }

        public SecurityNegotiationException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected SecurityNegotiationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
