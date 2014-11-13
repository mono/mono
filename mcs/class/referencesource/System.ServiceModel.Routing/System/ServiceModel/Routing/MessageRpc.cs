//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Routing
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.Transactions;
    using SR2 = System.ServiceModel.Routing.SR;
    using System.Security.Principal;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Diagnostics;

    // This class wraps a Message, MessageBuffer (if requested), and the OperationContext
    // The message is not buffered if nobody calls MessageRpc.CreateBuffer.  If the message
    // is buffered, then we hang on to the buffer so it can be reused and Clone the message
    // which became invalid when we buffered this message.
    class MessageRpc
    {
        const int ERROR_BAD_IMPERSONATION_LEVEL = 1346;

        Message originalMessage;
        Message clonedMessage;
        MessageBuffer messageBuffer;
        string uniqueID;
        Transaction transaction;
        ReceiveContext receiveContext;
        IList<SendOperation> operations;
        WindowsIdentity windowsIdentity;
        EventTraceActivity eventTraceActivity;

        public MessageRpc(Message message, OperationContext operationContext, bool impersonationRequired)
        {
            Fx.Assert(message != null, "message cannot be null");
            Fx.Assert(operationContext != null, "operationContext cannot be null");

            this.originalMessage = message;
            this.OperationContext = operationContext;

            if (Fx.Trace.IsEtwProviderEnabled)
            {
                this.eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(message);
            }

            //passing in true causes this to return null if the thread is not impersonating.
            this.windowsIdentity = WindowsIdentity.GetCurrent(true);
            if (impersonationRequired && !AspNetEnvironment.Current.AspNetCompatibilityEnabled)
            {
                if (this.windowsIdentity == null || this.windowsIdentity.ImpersonationLevel != TokenImpersonationLevel.Impersonation)
                {
                    //Temporarily revert impersonation to process token to throw an exception
                    IDisposable autoRevert = null;
                    try
                    {
                        try { }
                        finally
                        {
                            autoRevert = WindowsIdentity.Impersonate(IntPtr.Zero);
                        }

                        Win32Exception errorDetail = new Win32Exception(ERROR_BAD_IMPERSONATION_LEVEL);
                        throw FxTrace.Exception.AsError(new SecurityNegotiationException(errorDetail.Message));
                    }
                    finally
                    {
                        if (autoRevert != null)
                        {
                            autoRevert.Dispose();
                        }
                    }
                }
            }

            ReceiveContext.TryGet(message, out this.receiveContext);

            this.transaction = Transaction.Current;
            if (this.transaction == null)
            {
                this.transaction = TransactionMessageProperty.TryGetTransaction(message);
            }
        }

        internal bool Impersonating
        {
            get { return this.windowsIdentity != null; }
        }

        internal EventTraceActivity EventTraceActivity
        {
            get
            {
                return this.eventTraceActivity;
            }
        }

        public OperationContext OperationContext
        {
            get;
            private set;
        }

        public string UniqueID
        {
            get
            {
                if (this.uniqueID == null)
                {
                    if (this.Message.Version != MessageVersion.None &&
                        this.Message.Headers.MessageId != null)
                    {
                        this.uniqueID = this.originalMessage.Headers.MessageId.ToString();
                    }
                    else
                    {
                        this.uniqueID = this.GetHashCode().ToString(CultureInfo.InvariantCulture);
                    }
                }
                return this.uniqueID;
            }
        }

        public Message Message
        {
            get
            {
                // If we've created a MessageBuffer then the originalMessage has already been consumed
                if (this.messageBuffer != null)
                {
                    Fx.Assert(this.clonedMessage != null, "Need to set clonedMessage if we buffered the message");
                    return this.clonedMessage;
                }
                else
                {
                    // Haven't buffered, can use the original.
                    return this.originalMessage;
                }
            }
        }

        public ReceiveContext ReceiveContext
        {
            get { return this.receiveContext; }
        }

        public IList<SendOperation> Operations
        {
            get { return this.operations; }
        }

        public Transaction Transaction
        {
            get { return this.transaction; }
        }

        public MessageBuffer CreateBuffer()
        {
            if (this.messageBuffer == null)
            {
                this.messageBuffer = this.originalMessage.CreateBufferedCopy(int.MaxValue);
                this.clonedMessage = this.messageBuffer.CreateMessage();
            }
            return this.messageBuffer;
        }

        public IDisposable PrepareCall()
        {
            return new CallState(this);
        }

        public void RouteToSingleEndpoint<TContract>(RoutingConfiguration routingConfig)
        {
            IEnumerable<ServiceEndpoint> result;
            if (routingConfig.RouteOnHeadersOnly)
            {
                if (TD.RoutingServiceFilterTableMatchStartIsEnabled()) { TD.RoutingServiceFilterTableMatchStart(this.eventTraceActivity); }
                routingConfig.InternalFilterTable.GetMatchingValue(this.Message, out result);
                if (TD.RoutingServiceFilterTableMatchStopIsEnabled()) { TD.RoutingServiceFilterTableMatchStop(this.eventTraceActivity); }
            }
            else
            {
                MessageBuffer buffer = this.CreateBuffer();

                if (TD.RoutingServiceFilterTableMatchStartIsEnabled()) { TD.RoutingServiceFilterTableMatchStart(this.eventTraceActivity); }
                routingConfig.InternalFilterTable.GetMatchingValue(buffer, out result);
                if (TD.RoutingServiceFilterTableMatchStopIsEnabled()) { TD.RoutingServiceFilterTableMatchStop(this.eventTraceActivity); }
            }

            if (result == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR2.NoFilterMatched));
            }

            if (TD.RoutingServiceMessageRoutedToEndpointsIsEnabled())
            {
                TD.RoutingServiceMessageRoutedToEndpoints(this.eventTraceActivity, this.UniqueID, "1");
            }

            this.operations = new List<SendOperation>(1);
            this.operations.Add(new SendOperation(result, typeof(TContract), this.OperationContext));
        }

        public void RouteToEndpoints<TContract>(RoutingConfiguration routingConfig)
        {
            List<IEnumerable<ServiceEndpoint>> endpointLists = new List<IEnumerable<ServiceEndpoint>>();
            if (routingConfig.RouteOnHeadersOnly)
            {
                if (TD.RoutingServiceFilterTableMatchStartIsEnabled()) { TD.RoutingServiceFilterTableMatchStart(this.eventTraceActivity); }
                routingConfig.InternalFilterTable.GetMatchingValues(this.Message, endpointLists);
                if (TD.RoutingServiceFilterTableMatchStopIsEnabled()) { TD.RoutingServiceFilterTableMatchStop(this.eventTraceActivity); }
            }
            else
            {
                MessageBuffer messageBuffer = this.CreateBuffer();

                if (TD.RoutingServiceFilterTableMatchStartIsEnabled()) { TD.RoutingServiceFilterTableMatchStart(this.eventTraceActivity); }
                routingConfig.InternalFilterTable.GetMatchingValues(messageBuffer, endpointLists);
                if (TD.RoutingServiceFilterTableMatchStopIsEnabled()) { TD.RoutingServiceFilterTableMatchStop(this.eventTraceActivity); }
            }
            
            if (TD.RoutingServiceMessageRoutedToEndpointsIsEnabled())
            {
                TD.RoutingServiceMessageRoutedToEndpoints(this.eventTraceActivity, this.UniqueID, endpointLists.Count.ToString(TD.Culture));
            }
            if (endpointLists.Count == 0)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR2.NoFilterMatched));
            }
            this.operations = new List<SendOperation>(endpointLists.Count);
            foreach (IEnumerable<ServiceEndpoint> endpointList in endpointLists)
            {
                this.operations.Add(new SendOperation(endpointList, typeof(TContract), this.OperationContext));
            }
        }

        class CallState : IDisposable
        {
            OperationContextScope nullContextScope;
            WindowsImpersonationContext impersonation;

            public CallState (MessageRpc messageRpc)
            {
                this.nullContextScope = new OperationContextScope((OperationContext)null);

                if (messageRpc.windowsIdentity != null)
                {
                    this.impersonation = messageRpc.windowsIdentity.Impersonate();
                }
            }

            void IDisposable.Dispose()
            {
                if (this.impersonation != null)
                {
                    this.impersonation.Dispose();
                }
                this.nullContextScope.Dispose();
            }
        }
    }
}
