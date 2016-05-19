//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System.Runtime.Serialization;

    [Serializable]
    public class AddressAlreadyInUseException : CommunicationException
    {
        public AddressAlreadyInUseException() { }
        public AddressAlreadyInUseException(string message) : base(message) { }
        public AddressAlreadyInUseException(string message, Exception innerException) : base(message, innerException) { }
        protected AddressAlreadyInUseException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
