//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Diagnostics
{
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    class AddressingProperty
    {
        string action;
        Uri to;
        EndpointAddress replyTo;
        System.Xml.UniqueId messageId;

        public AddressingProperty(MessageHeaders headers)
        {
            Fx.Assert(null != headers, "");

            this.action = headers.Action;
            this.to = headers.To;
            this.replyTo = headers.ReplyTo;
            this.messageId = headers.MessageId;
        }

        public string Action
        {
            get { return this.action; }
        }

        public UniqueId MessageId
        {
            get { return this.messageId; }
        }

        public static string Name
        {
            get { return MessageLogTraceRecord.AddressingElementName; }
        }

        public EndpointAddress ReplyTo
        {
            get { return this.replyTo; }
        }

        public Uri To
        {
            get { return this.to; }
        }
    }
}
