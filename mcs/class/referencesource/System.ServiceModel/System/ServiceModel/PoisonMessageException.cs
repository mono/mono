//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System.Runtime.Serialization;

    [Serializable]
    public class PoisonMessageException : CommunicationException
    {
        public PoisonMessageException() { }
        public PoisonMessageException(string message) : base(message) { }
        public PoisonMessageException(string message, Exception innerException) : base(message, innerException) { }
        protected PoisonMessageException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
