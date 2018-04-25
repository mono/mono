//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    class BufferedRequestContext : RequestContext
    {
        bool delayClose;
        object thisLock;
        RequestContext innerRequestContext;


        public BufferedRequestContext(RequestContext requestContext)
        {
            this.innerRequestContext = requestContext;
            this.thisLock = new object();
        }

        public override Message RequestMessage
        {
            get
            {
                return innerRequestContext.RequestMessage;
            }
        }

        public RequestContext InnerRequestContext
        {
            get
            {
                return innerRequestContext;
            }
        }

        public void DelayClose(bool delay)
        {
            lock (this.thisLock)
            {
                this.delayClose = delay;
            }
        }

        public void ReInitialize(Message requestMessage)
        {
            // things might ---- up here if a custom channel is using any properties on the original message...
            // we should consider creating a virtual method in Dev11 on RequestContext to allow authors of custom
            // channels reset the state of the request context before retrying a message.
            RequestContextBase requestContextBase = this.innerRequestContext as RequestContextBase;
            if (requestContextBase != null)
            {
                requestContextBase.ReInitialize(requestMessage);
            }
        }

        public override void Abort()
        {
            lock (this.thisLock)
            {
                if (this.delayClose)
                {
                    // Only delay the first attempt at Close/Abort
                    this.delayClose = false;
                    return;
                }
            }
            this.innerRequestContext.Abort();
        }

        public override void Close()
        {
            lock (this.thisLock)
            {
                if (this.delayClose)
                {
                    // Only delay the first attempt at Close/Abort
                    this.delayClose = false;
                    return;
                }
            }
            this.innerRequestContext.Close();
        }

        public override void Close(TimeSpan timeout)
        {
            lock (this.thisLock)
            {
                if (this.delayClose)
                {
                    // Only delay the first attempt at Close/Abort
                    this.delayClose = false;
                    return;
                }
            }
            this.innerRequestContext.Close(timeout);
        }


        public override void Reply(Message message)
        {
            this.innerRequestContext.Reply(message);
        }

        public override void Reply(Message message, TimeSpan timeout)
        {
            this.innerRequestContext.Reply(message, timeout);
        }

        public override IAsyncResult BeginReply(Message message, AsyncCallback callback, object state)
        {
            return this.innerRequestContext.BeginReply(message, callback, state);
        }

        public override IAsyncResult BeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.innerRequestContext.BeginReply(message, timeout, callback, state);
        }

        public override void EndReply(IAsyncResult result)
        {
            this.innerRequestContext.EndReply(result);
        }
    }
}
