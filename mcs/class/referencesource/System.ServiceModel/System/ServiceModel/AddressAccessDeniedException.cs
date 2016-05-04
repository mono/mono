//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System.Runtime.Serialization;

    [Serializable]
    public class AddressAccessDeniedException : CommunicationException
    {
        public AddressAccessDeniedException() { }
        public AddressAccessDeniedException(string message) : base(message) { }
        public AddressAccessDeniedException(string message, Exception innerException) : base(message, innerException) { }
        protected AddressAccessDeniedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
