//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.ServiceModel.Diagnostics;

    abstract class ClientReliableChannelBinder<TChannel> : ReliableChannelBinder<TChannel>,
        IClientReliableChannelBinder
        where TChannel : class, IChannel
    {
        ChannelParameterCollection channelParameters;
        IChannelFactory<TChannel> factory;
        EndpointAddress to;
        Uri via;

        protected ClientReliableChannelBinder(EndpointAddress to, Uri via, IChannelFactory<TChannel> factory,
            MaskingMode maskingMode, TolerateFaultsMode faultMode, ChannelParameterCollection channelParameters,
            TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout)
            : base(factory.CreateChannel(to, via), maskingMode, faultMode,
            defaultCloseTimeout, defaultSendTimeout)
        {
            if (channelParameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("channelParameters");
            }

            this.to = to;
            this.via = via;
            this.factory = factory;
            this.channelParameters = channelParameters;
        }

        // The server side must get a message to determine where the channel should go, thus it is
        // pointless to create a channel for the sake of receiving on the client side. Also, since
        // the client side can create channels there receive may enter an infinite loop if open
        // persistently throws.
        protected override bool CanGetChannelForReceive
        {
            get
            {
                return false;
            }
        }

        public override bool CanSendAsynchronously
        {
            get
            {
                return true;
            }
        }

        public override ChannelParameterCollection ChannelParameters
        {
            get
            {
                return this.channelParameters;
            }
        }

        protected override bool MustCloseChannel
        {
            get
            {
                return true;
            }
        }

        protected override bool MustOpenChannel
        {
            get
            {
                return true;
            }
        }

        public Uri Via
        {
            get
            {
                return this.via;
            }
        }

        public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback,
            object state)
        {
            return this.BeginRequest(message, timeout, this.DefaultMaskingMode, callback, state);
        }

        public IAsyncResult BeginRequest(Message message, TimeSpan timeout, MaskingMode maskingMode,
            AsyncCallback callback, object state)
        {
            RequestAsyncResult result = new RequestAsyncResult(this, callback, state);
            result.Start(message, timeout, maskingMode);
            return result;
        }

        protected override IAsyncResult BeginTryGetChannel(TimeSpan timeout,
            AsyncCallback callback, object state)
        {
            CommunicationState currentState = this.State;
            TChannel channel;

            if ((currentState == CommunicationState.Created)
               || (currentState == CommunicationState.Opening)
               || (currentState == CommunicationState.Opened))
            {
                channel = this.factory.CreateChannel(this.to, this.via);
            }
            else
            {
                channel = null;
            }

            return new CompletedAsyncResult<TChannel>(channel, callback, state);
        }

        public static IClientReliableChannelBinder CreateBinder(EndpointAddress to, Uri via,
            IChannelFactory<TChannel> factory, MaskingMode maskingMode, TolerateFaultsMode faultMode,
            ChannelParameterCollection channelParameters,
            TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout)
        {
            Type type = typeof(TChannel);

            if (type == typeof(IDuplexChannel))
            {
                return new DuplexClientReliableChannelBinder(to, via, (IChannelFactory<IDuplexChannel>)(object)factory, maskingMode,
                    channelParameters, defaultCloseTimeout, defaultSendTimeout);
            }
            else if (type == typeof(IDuplexSessionChannel))
            {
                return new DuplexSessionClientReliableChannelBinder(to, via, (IChannelFactory<IDuplexSessionChannel>)(object)factory, maskingMode,
                    faultMode, channelParameters, defaultCloseTimeout, defaultSendTimeout);
            }
            else if (type == typeof(IRequestChannel))
            {
                return new RequestClientReliableChannelBinder(to, via, (IChannelFactory<IRequestChannel>)(object)factory, maskingMode,
                    channelParameters, defaultCloseTimeout, defaultSendTimeout);
            }
            else if (type == typeof(IRequestSessionChannel))
            {
                return new RequestSessionClientReliableChannelBinder(to, via, (IChannelFactory<IRequestSessionChannel>)(object)factory, maskingMode,
                    faultMode, channelParameters, defaultCloseTimeout, defaultSendTimeout);
            }
            else
            {
                throw Fx.AssertAndThrow("ClientReliableChannelBinder supports creation of IDuplexChannel, IDuplexSessionChannel, IRequestChannel, and IRequestSessionChannel only.");
            }
        }

        public Message EndRequest(IAsyncResult result)
        {
            return RequestAsyncResult.End(result);
        }

        protected override bool EndTryGetChannel(IAsyncResult result)
        {
            TChannel channel = CompletedAsyncResult<TChannel>.End(result);

            if (channel != null && !this.Synchronizer.SetChannel(channel))
            {
                channel.Abort();
            }

            return true;
        }

        public bool EnsureChannelForRequest()
        {
            return this.Synchronizer.EnsureChannel();
        }

        protected override void OnAbort()
        {
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback,
            object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback,
            object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected virtual IAsyncResult OnBeginRequest(TChannel channel, Message message,
            TimeSpan timeout, MaskingMode maskingMode, AsyncCallback callback, object state)
        {
            throw Fx.AssertAndThrow("The derived class does not support the OnBeginRequest operation.");
        }

        protected override void OnClose(TimeSpan timeout)
        {
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected virtual Message OnEndRequest(TChannel channel, MaskingMode maskingMode,
            IAsyncResult result)
        {
            throw Fx.AssertAndThrow("The derived class does not support the OnEndRequest operation.");
        }

        protected override void OnOpen(TimeSpan timeout)
        {
        }

        protected virtual Message OnRequest(TChannel channel, Message message, TimeSpan timeout,
            MaskingMode maskingMode)
        {
            throw Fx.AssertAndThrow("The derived class does not support the OnRequest operation.");
        }

        public Message Request(Message message, TimeSpan timeout)
        {
            return this.Request(message, timeout, this.DefaultMaskingMode);
        }

        public Message Request(Message message, TimeSpan timeout, MaskingMode maskingMode)
        {
            if (!this.ValidateOutputOperation(message, timeout, maskingMode))
            {
                return null;
            }

            bool autoAborted = false;

            try
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                TChannel channel;

                if (!this.Synchronizer.TryGetChannelForOutput(timeoutHelper.RemainingTime(), maskingMode,
                    out channel))
                {
                    if (!ReliableChannelBinderHelper.MaskHandled(maskingMode))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new TimeoutException(SR.GetString(SR.TimeoutOnRequest, timeout)));
                    }

                    return null;
                }

                if (channel == null)
                {
                    return null;
                }

                try
                {
                    return this.OnRequest(channel, message, timeoutHelper.RemainingTime(),
                        maskingMode);
                }
                finally
                {
                    autoAborted = this.Synchronizer.Aborting;
                    this.Synchronizer.ReturnChannel();
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;

                if (!this.HandleException(e, maskingMode, autoAborted))
                {
                    throw;
                }
                else
                {
                    return null;
                }
            }
        }

        protected override bool TryGetChannel(TimeSpan timeout)
        {
            CommunicationState currentState = this.State;
            TChannel channel = null;

            if ((currentState == CommunicationState.Created)
               || (currentState == CommunicationState.Opening)
               || (currentState == CommunicationState.Opened))
            {
                channel = this.factory.CreateChannel(this.to, this.via);
                if (!this.Synchronizer.SetChannel(channel))
                {
                    channel.Abort();
                }
            }
            else
            {
                channel = null;
            }

            return true;
        }

        abstract class DuplexClientReliableChannelBinder<TDuplexChannel>
            : ClientReliableChannelBinder<TDuplexChannel>
            where TDuplexChannel : class, IDuplexChannel
        {
            public DuplexClientReliableChannelBinder(EndpointAddress to, Uri via,
                IChannelFactory<TDuplexChannel> factory, MaskingMode maskingMode, TolerateFaultsMode faultMode,
                ChannelParameterCollection channelParameters,
                TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout)
                : base(to, via, factory, maskingMode, faultMode, channelParameters, defaultCloseTimeout,
                defaultSendTimeout)
            {
            }

            public override EndpointAddress LocalAddress
            {
                get
                {
                    IDuplexChannel channel = this.Synchronizer.CurrentChannel;
                    if (channel == null)
                        return null;
                    else
                        return channel.LocalAddress;
                }
            }

            public override EndpointAddress RemoteAddress
            {
                get
                {
                    IDuplexChannel channel = this.Synchronizer.CurrentChannel;
                    if (channel == null)
                        return null;
                    else
                        return channel.RemoteAddress;
                }
            }

            protected override IAsyncResult OnBeginSend(TDuplexChannel channel, Message message,
                TimeSpan timeout, AsyncCallback callback, object state)
            {
                return channel.BeginSend(message, timeout, callback, state);
            }

            protected override IAsyncResult OnBeginTryReceive(TDuplexChannel channel,
                TimeSpan timeout, AsyncCallback callback, object state)
            {
                return channel.BeginTryReceive(timeout, callback, state);
            }

            protected override void OnEndSend(TDuplexChannel channel, IAsyncResult result)
            {
                channel.EndSend(result);
            }

            protected override bool OnEndTryReceive(TDuplexChannel channel, IAsyncResult result,
                out RequestContext requestContext)
            {
                Message message;
                bool success = channel.EndTryReceive(result, out message);
                if (success && message == null)
                {
                    this.OnReadNullMessage();
                }
                requestContext = this.WrapMessage(message);
                return success;
            }

            protected virtual void OnReadNullMessage()
            {
            }

            protected override void OnSend(TDuplexChannel channel, Message message,
                TimeSpan timeout)
            {
                channel.Send(message, timeout);
            }

            protected override bool OnTryReceive(TDuplexChannel channel, TimeSpan timeout,
                out RequestContext requestContext)
            {
                Message message;
                bool success = channel.TryReceive(timeout, out message);
                if (success && message == null)
                {
                    this.OnReadNullMessage();
                }
                requestContext = this.WrapMessage(message);
                return success;
            }
        }

        sealed class DuplexClientReliableChannelBinder
            : DuplexClientReliableChannelBinder<IDuplexChannel>
        {
            public DuplexClientReliableChannelBinder(EndpointAddress to, Uri via,
                IChannelFactory<IDuplexChannel> factory, MaskingMode maskingMode,
                ChannelParameterCollection channelParameters,
                TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout)
                : base(to, via, factory, maskingMode, TolerateFaultsMode.Never, channelParameters,
                defaultCloseTimeout, defaultSendTimeout)
            {
            }

            public override bool HasSession
            {
                get
                {
                    return false;
                }
            }

            public override ISession GetInnerSession()
            {
                return null;
            }

            protected override bool HasSecuritySession(IDuplexChannel channel)
            {
                return false;
            }
        }

        sealed class DuplexSessionClientReliableChannelBinder
            : DuplexClientReliableChannelBinder<IDuplexSessionChannel>
        {
            public DuplexSessionClientReliableChannelBinder(EndpointAddress to, Uri via,
                IChannelFactory<IDuplexSessionChannel> factory, MaskingMode maskingMode, TolerateFaultsMode faultMode,
                ChannelParameterCollection channelParameters,
                TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout)
                : base(to, via, factory, maskingMode, faultMode, channelParameters, defaultCloseTimeout,
                defaultSendTimeout)
            {
            }

            public override bool HasSession
            {
                get
                {
                    return true;
                }
            }

            public override ISession GetInnerSession()
            {
                return this.Synchronizer.CurrentChannel.Session;
            }

            protected override IAsyncResult BeginCloseChannel(IDuplexSessionChannel channel,
                TimeSpan timeout, AsyncCallback callback, object state)
            {
                return ReliableChannelBinderHelper.BeginCloseDuplexSessionChannel(this, channel,
                    timeout, callback, state);
            }

            protected override void CloseChannel(IDuplexSessionChannel channel, TimeSpan timeout)
            {
                ReliableChannelBinderHelper.CloseDuplexSessionChannel(this, channel, timeout);
            }

            protected override void EndCloseChannel(IDuplexSessionChannel channel,
                IAsyncResult result)
            {
                ReliableChannelBinderHelper.EndCloseDuplexSessionChannel(channel, result);
            }

            protected override bool HasSecuritySession(IDuplexSessionChannel channel)
            {
                return channel.Session is ISecuritySession;
            }

            protected override void OnReadNullMessage()
            {
                this.Synchronizer.OnReadEof();
            }
        }

        abstract class RequestClientReliableChannelBinder<TRequestChannel>
            : ClientReliableChannelBinder<TRequestChannel>
            where TRequestChannel : class, IRequestChannel
        {
            InputQueue<Message> inputMessages;

            public RequestClientReliableChannelBinder(EndpointAddress to, Uri via,
                IChannelFactory<TRequestChannel> factory, MaskingMode maskingMode, TolerateFaultsMode faultMode,
                ChannelParameterCollection channelParameters,
                TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout)
                : base(to, via, factory, maskingMode, faultMode, channelParameters, defaultCloseTimeout,
                defaultSendTimeout)
            {
            }

            public override IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback,
                object state)
            {
                return this.GetInputMessages().BeginDequeue(timeout, callback, state);
            }

            public override bool EndTryReceive(IAsyncResult result,
                out RequestContext requestContext)
            {
                Message message;
                bool success = this.GetInputMessages().EndDequeue(result, out message);
                requestContext = this.WrapMessage(message);
                return success;
            }

            protected void EnqueueMessageIfNotNull(Message message)
            {
                if (message != null)
                {
                    this.GetInputMessages().EnqueueAndDispatch(message);
                }
            }

            InputQueue<Message> GetInputMessages()
            {
                lock (this.ThisLock)
                {
                    if (this.State == CommunicationState.Created)
                    {
                        throw Fx.AssertAndThrow("The method GetInputMessages() cannot be called when the binder is in the Created state.");
                    }

                    if (this.State == CommunicationState.Opening)
                    {
                        throw Fx.AssertAndThrow("The method GetInputMessages() cannot be called when the binder is in the Opening state.");
                    }

                    if (this.inputMessages == null)
                    {
                        this.inputMessages = TraceUtility.CreateInputQueue<Message>();
                    }
                }

                return this.inputMessages;
            }

            public override EndpointAddress LocalAddress
            {
                get
                {
                    return EndpointAddress.AnonymousAddress;
                }
            }

            public override EndpointAddress RemoteAddress
            {
                get
                {
                    IRequestChannel channel = this.Synchronizer.CurrentChannel;
                    if (channel == null)
                        return null;
                    else
                        return channel.RemoteAddress;
                }
            }

            protected override IAsyncResult OnBeginRequest(TRequestChannel channel,
                Message message, TimeSpan timeout, MaskingMode maskingMode,
                AsyncCallback callback, object state)
            {
                return channel.BeginRequest(message, timeout, callback, state);
            }

            protected override IAsyncResult OnBeginSend(TRequestChannel channel, Message message,
                TimeSpan timeout, AsyncCallback callback, object state)
            {
                return channel.BeginRequest(message, timeout, callback, state);
            }

            protected override Message OnEndRequest(TRequestChannel channel,
                MaskingMode maskingMode, IAsyncResult result)
            {
                return channel.EndRequest(result);
            }

            protected override void OnEndSend(TRequestChannel channel, IAsyncResult result)
            {
                Message message = channel.EndRequest(result);
                this.EnqueueMessageIfNotNull(message);
            }

            protected override Message OnRequest(TRequestChannel channel, Message message,
                TimeSpan timeout, MaskingMode maskingMode)
            {
                return channel.Request(message, timeout);
            }

            protected override void OnSend(TRequestChannel channel, Message message,
                TimeSpan timeout)
            {
                message = channel.Request(message, timeout);
                this.EnqueueMessageIfNotNull(message);
            }

            protected override void OnShutdown()
            {
                if (this.inputMessages != null)
                {
                    this.inputMessages.Close();
                }
            }

            public override bool TryReceive(TimeSpan timeout, out RequestContext requestContext)
            {
                Message message;
                bool success = this.GetInputMessages().Dequeue(timeout, out message);
                requestContext = this.WrapMessage(message);
                return success;
            }
        }

        sealed class RequestAsyncResult
            : ReliableChannelBinder<TChannel>.OutputAsyncResult<ClientReliableChannelBinder<TChannel>>
        {
            Message reply;

            public RequestAsyncResult(ClientReliableChannelBinder<TChannel> binder,
                AsyncCallback callback, object state)
                : base(binder, callback, state)
            {
            }

            protected override IAsyncResult BeginOutput(
                ClientReliableChannelBinder<TChannel> binder, TChannel channel, Message message,
                TimeSpan timeout, MaskingMode maskingMode, AsyncCallback callback, object state)
            {
                return binder.OnBeginRequest(channel, message, timeout, maskingMode, callback,
                    state);
            }

            public static Message End(IAsyncResult result)
            {
                RequestAsyncResult requestResult = AsyncResult.End<RequestAsyncResult>(result);
                return requestResult.reply;
            }

            protected override void EndOutput(ClientReliableChannelBinder<TChannel> binder,
                TChannel channel, MaskingMode maskingMode, IAsyncResult result)
            {
                this.reply = binder.OnEndRequest(channel, maskingMode, result);
            }

            protected override string GetTimeoutString(TimeSpan timeout)
            {
                return SR.GetString(SR.TimeoutOnRequest, timeout);
            }
        }

        sealed class RequestClientReliableChannelBinder
           : RequestClientReliableChannelBinder<IRequestChannel>
        {
            public RequestClientReliableChannelBinder(EndpointAddress to, Uri via,
                IChannelFactory<IRequestChannel> factory, MaskingMode maskingMode,
                ChannelParameterCollection channelParameters,
                TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout)
                : base(to, via, factory, maskingMode, TolerateFaultsMode.Never, channelParameters,
                defaultCloseTimeout, defaultSendTimeout)
            {
            }

            public override bool HasSession
            {
                get
                {
                    return false;
                }
            }

            public override ISession GetInnerSession()
            {
                return null;
            }

            protected override bool HasSecuritySession(IRequestChannel channel)
            {
                return false;
            }
        }

        sealed class RequestSessionClientReliableChannelBinder
            : RequestClientReliableChannelBinder<IRequestSessionChannel>
        {
            public RequestSessionClientReliableChannelBinder(EndpointAddress to, Uri via,
                IChannelFactory<IRequestSessionChannel> factory, MaskingMode maskingMode, TolerateFaultsMode faultMode,
                ChannelParameterCollection channelParameters,
                TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout)
                : base(to, via, factory, maskingMode, faultMode, channelParameters, defaultCloseTimeout,
                defaultSendTimeout)
            {
            }

            public override bool HasSession
            {
                get
                {
                    return true;
                }
            }

            public override ISession GetInnerSession()
            {
                return this.Synchronizer.CurrentChannel.Session;
            }

            protected override bool HasSecuritySession(IRequestSessionChannel channel)
            {
                return channel.Session is ISecuritySession;
            }
        }
    }
}
