//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    class CompositeDispatchFormatter : IDispatchMessageFormatter
    {
        IDispatchMessageFormatter reply;
        IDispatchMessageFormatter request;
        public CompositeDispatchFormatter(IDispatchMessageFormatter request, IDispatchMessageFormatter reply)
        {
            this.request = request;
            this.reply = reply;
        }

        public void DeserializeRequest(Message message, object[] parameters)
        {
            this.request.DeserializeRequest(message, parameters);
        }

        public Message SerializeReply(MessageVersion messageVersion, object[] parameters, object result)
        {
            return this.reply.SerializeReply(messageVersion, parameters, result);
        }
    }
}

