//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Activities.Dispatcher;
    using System.ServiceModel.Channels;

    // These are information passed from Send to Receive
    class CorrelationRequestContext
    {
        AsyncWaitHandle receivedReplyEvent;
        Exception exceptionOnReply;

        public CorrelationRequestContext()
        {
        }

        public OperationContext OperationContext
        {
            get;
            set;
        }

        public Message Reply
        {
            get;
            set;
        }

        public Exception Exception
        {
            get;
            set;
        }

        public CorrelationKeyCalculator CorrelationKeyCalculator
        {
            get;
            set;
        }

        public void EnsureAsyncWaitHandle()
        {
            this.receivedReplyEvent = new AsyncWaitHandle();
        }

        public bool TryGetReply()
        {
            if (this.exceptionOnReply != null)
            {
                throw FxTrace.Exception.AsError(this.exceptionOnReply);
            }

            return this.Reply != null;
        }

        public bool WaitForReplyAsync(Action<object, TimeoutException> onReceiveReply, object state)
        {
            Fx.Assert(this.receivedReplyEvent != null, "AsyncWaitHandle must be initialized before this point!");

            if (TryGetReply())
            {
                return true;
            }

            return this.receivedReplyEvent.WaitAsync(onReceiveReply, state, TimeSpan.MaxValue);
        }
        
        // This is only called on the synchronous code path
        public void ReceiveReply(OperationContext operationContext, Message reply)
        {
            this.OperationContext = operationContext;
            this.Reply = reply;
        }

        // This is called on the async code path
        public void ReceiveAsyncReply(OperationContext operationContext, Message reply, Exception replyException)
        {
            Fx.Assert(this.receivedReplyEvent != null, "AsyncWaitHandle must be initialized before this point!");

            this.OperationContext = operationContext;
            this.exceptionOnReply = replyException;

            // NOTE: we make sure that this.Reply is set after the operation context since we 
            // pivot off of this fact in InternalReceiveMessage to optimize out an AsyncResult
            // allocation. If you have more data to populate in ReceiveAsyncReply, then you need
            // to make those assignments before we populate this.Reply
            this.Reply = reply;

            this.receivedReplyEvent.Set();
        }

        public void Cancel()
        {
            Fx.Assert(this.receivedReplyEvent != null, "AsyncWaitHandle must be initialized before this point!");

            this.receivedReplyEvent.Set();
        }
    }
}
