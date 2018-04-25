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
    internal class ActionMismatchAddressingException : ProtocolException
    {
        string httpActionHeader;
        string soapActionHeader;

        public ActionMismatchAddressingException(string message, string soapActionHeader, string httpActionHeader)
            : base(message)
        {
            this.httpActionHeader = httpActionHeader;
            this.soapActionHeader = soapActionHeader;
        }

        protected ActionMismatchAddressingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string HttpActionHeader
        {
            get
            {
                return httpActionHeader;
            }
        }

        public string SoapActionHeader
        {
            get
            {
                return soapActionHeader;
            }
        }

        internal Message ProvideFault(MessageVersion messageVersion)
        {
            Fx.Assert(messageVersion.Addressing == AddressingVersion.WSAddressing10, "");
            WSAddressing10ProblemHeaderQNameFault phf = new WSAddressing10ProblemHeaderQNameFault(this);
            Message message = System.ServiceModel.Channels.Message.CreateMessage(messageVersion, phf, messageVersion.Addressing.FaultAction);
            phf.AddHeaders(message.Headers);
            return message;
        }
    }
}
