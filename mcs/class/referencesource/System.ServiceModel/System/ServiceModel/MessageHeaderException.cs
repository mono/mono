//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.ServiceModel.Channels;

    [Serializable]
    public class MessageHeaderException : ProtocolException
    {
        [NonSerialized]
        string headerName;
        [NonSerialized]
        string headerNamespace;
        [NonSerialized]
        bool isDuplicate;

        public MessageHeaderException(string message)
            : this(message, null, null)
        {
        }
        public MessageHeaderException(string message, bool isDuplicate)
            : this(message, null, null)
        {
        }
        public MessageHeaderException(string message, Exception innerException)
            : this(message, null, null, innerException)
        {
        }
        public MessageHeaderException(string message, string headerName, string ns)
            : this(message, headerName, ns, null)
        {
        }
        public MessageHeaderException(string message, string headerName, string ns, bool isDuplicate)
            : this(message, headerName, ns, isDuplicate, null)
        {
        }
        public MessageHeaderException(string message, string headerName, string ns, Exception innerException)
            : this(message, headerName, ns, false, innerException)
        {
        }
        public MessageHeaderException(string message, string headerName, string ns, bool isDuplicate, Exception innerException)
            : base(message, innerException)
        {
            this.headerName = headerName;
            this.headerNamespace = ns;
            this.isDuplicate = isDuplicate;
        }

        public string HeaderName { get { return this.headerName; } }

        public string HeaderNamespace { get { return this.headerNamespace; } }

        // IsDuplicate==true means there was more than one; IsDuplicate==false means there were zero
        public bool IsDuplicate { get { return this.isDuplicate; } }

        internal Message ProvideFault(MessageVersion messageVersion)
        {
            Fx.Assert(messageVersion.Addressing == AddressingVersion.WSAddressing10, "");
            WSAddressing10ProblemHeaderQNameFault phf = new WSAddressing10ProblemHeaderQNameFault(this);
            Message message = System.ServiceModel.Channels.Message.CreateMessage(messageVersion, phf, AddressingVersion.WSAddressing10.FaultAction);
            phf.AddHeaders(message.Headers);
            return message;
        }

        // for serialization
        public MessageHeaderException() { }
        protected MessageHeaderException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
