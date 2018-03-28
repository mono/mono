//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class ChannelTerminatedException : CommunicationException
    {
        public ChannelTerminatedException() { }
        public ChannelTerminatedException(string message) : base(message) { }
        public ChannelTerminatedException(string message, Exception innerException) : base(message, innerException) { }
        protected ChannelTerminatedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
