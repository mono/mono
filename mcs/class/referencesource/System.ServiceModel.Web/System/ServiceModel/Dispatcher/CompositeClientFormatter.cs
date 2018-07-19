//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    class CompositeClientFormatter : IClientMessageFormatter
    {
        IClientMessageFormatter reply;
        IClientMessageFormatter request;
        public CompositeClientFormatter(IClientMessageFormatter request, IClientMessageFormatter reply)
        {
            this.request = request;
            this.reply = reply;
        }

        public object DeserializeReply(Message message, object[] parameters)
        {
            return this.reply.DeserializeReply(message, parameters);
        }

        public Message SerializeRequest(MessageVersion messageVersion, object[] parameters)
        {
            return this.request.SerializeRequest(messageVersion, parameters);
        }
    }
}

