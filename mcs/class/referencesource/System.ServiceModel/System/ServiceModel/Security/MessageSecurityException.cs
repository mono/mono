//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Collections;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Text;
    using System.Xml;
    using System.Security;
    using System.Runtime;

    [Serializable]
    public class MessageSecurityException : CommunicationException
    {
        MessageFault fault;
        bool isReplay = false;

        public MessageSecurityException()
            : base()
        {
        }

        public MessageSecurityException(String message)
            : base(message)
        {
        }

        public MessageSecurityException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected MessageSecurityException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }

        internal MessageSecurityException(string message, Exception innerException, MessageFault fault)
            : base(message, innerException)
        {
            this.fault = fault;
        }

        internal MessageSecurityException(String message, bool isReplay)
            : base(message)
        {
            this.isReplay = isReplay;
        }

        internal bool ReplayDetected
        {
            get
            {
                return this.isReplay;
            }
        }

        internal MessageFault Fault
        {
            get { return this.fault; }
        }
    }
}
