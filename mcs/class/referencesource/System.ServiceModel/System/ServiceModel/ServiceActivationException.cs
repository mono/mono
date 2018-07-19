//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class ServiceActivationException : CommunicationException
    {
        public ServiceActivationException() { }
        public ServiceActivationException(string message) : base(message) { }
        public ServiceActivationException(string message, Exception innerException) : base(message, innerException) { }
        protected ServiceActivationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
