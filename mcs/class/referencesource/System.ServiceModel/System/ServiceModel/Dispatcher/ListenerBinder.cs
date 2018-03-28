//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel.Channels;

    static class ListenerBinder
    {
        internal static IListenerBinder GetBinder(IChannelListener listener, MessageVersion messageVersion)
        {
            IChannelListener<IInputChannel> input = listener as IChannelListener<IInputChannel>;
            if (input != null)
                return new InputListenerBinder(input, messageVersion);

            IChannelListener<IInputSessionChannel> inputSession = listener as IChannelListener<IInputSessionChannel>;
            if (inputSession != null)
                return new InputSessionListenerBinder(inputSession, messageVersion);

            IChannelListener<IReplyChannel> reply = listener as IChannelListener<IReplyChannel>;
            if (reply != null)
                return new ReplyListenerBinder(reply, messageVersion);

            IChannelListener<IReplySessionChannel> replySession = listener as IChannelListener<IReplySessionChannel>;
            if (replySession != null)
                return new ReplySessionListenerBinder(replySession, messageVersion);

            IChannelListener<IDuplexChannel> duplex = listener as IChannelListener<IDuplexChannel>;
            if (duplex != null)
                return new DuplexListenerBinder(duplex, messageVersion);

            IChannelListener<IDuplexSessionChannel> duplexSession = listener as IChannelListener<IDuplexSessionChannel>;
            if (duplexSession != null)
                return new DuplexSessionListenerBinder(duplexSession, messageVersion);

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.UnknownListenerType1, listener.Uri.AbsoluteUri)));
        }

        // ------------------------------------------------------------------------------------------------------------
        // Listener Binders

        class DuplexListenerBinder : IListenerBinder
        {
            IRequestReplyCorrelator correlator;
            IChannelListener<IDuplexChannel> listener;
            MessageVersion messageVersion;

            internal DuplexListenerBinder(IChannelListener<IDuplexChannel> listener, MessageVersion messageVersion)
            {
                this.correlator = new RequestReplyCorrelator();
                this.listener = listener;
                this.messageVersion = messageVersion;
            }

            public IChannelListener Listener
            {
                get { return this.listener; }
            }

            public MessageVersion MessageVersion
            {
                get { return this.messageVersion; }
            }

            public IChannelBinder Accept(TimeSpan timeout)
            {
                IDuplexChannel channel = this.listener.AcceptChannel(timeout);
                if (channel == null)
                    return null;

                return new DuplexChannelBinder(channel, this.correlator, this.listener.Uri);
            }

            public IAsyncResult BeginAccept(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.listener.BeginAcceptChannel(timeout, callback, state);
            }

            public IChannelBinder EndAccept(IAsyncResult result)
            {
                IDuplexChannel channel = this.listener.EndAcceptChannel(result);
                if (channel == null)
                    return null;

                return new DuplexChannelBinder(channel, this.correlator, this.listener.Uri);
            }
        }

        class DuplexSessionListenerBinder : IListenerBinder
        {
            IRequestReplyCorrelator correlator;
            IChannelListener<IDuplexSessionChannel> listener;
            MessageVersion messageVersion;

            internal DuplexSessionListenerBinder(IChannelListener<IDuplexSessionChannel> listener, MessageVersion messageVersion)
            {
                this.correlator = new RequestReplyCorrelator();
                this.listener = listener;
                this.messageVersion = messageVersion;
            }

            public IChannelListener Listener
            {
                get { return this.listener; }
            }

            public MessageVersion MessageVersion
            {
                get { return this.messageVersion; }
            }

            public IChannelBinder Accept(TimeSpan timeout)
            {
                IDuplexSessionChannel channel = this.listener.AcceptChannel(timeout);
                if (channel == null)
                    return null;

                return new DuplexChannelBinder(channel, this.correlator, this.listener.Uri);
            }

            public IAsyncResult BeginAccept(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.listener.BeginAcceptChannel(timeout, callback, state);
            }

            public IChannelBinder EndAccept(IAsyncResult result)
            {
                IDuplexSessionChannel channel = this.listener.EndAcceptChannel(result);
                if (channel == null)
                    return null;

                return new DuplexChannelBinder(channel, this.correlator, this.listener.Uri);
            }
        }

        class InputListenerBinder : IListenerBinder
        {
            IChannelListener<IInputChannel> listener;
            MessageVersion messageVersion;

            internal InputListenerBinder(IChannelListener<IInputChannel> listener, MessageVersion messageVersion)
            {
                this.listener = listener;
                this.messageVersion = messageVersion;
            }

            public IChannelListener Listener
            {
                get { return this.listener; }
            }

            public MessageVersion MessageVersion
            {
                get { return this.messageVersion; }
            }

            public IChannelBinder Accept(TimeSpan timeout)
            {
                IInputChannel channel = this.listener.AcceptChannel(timeout);
                if (channel == null)
                    return null;

                return new InputChannelBinder(channel, this.listener.Uri);
            }

            public IAsyncResult BeginAccept(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.listener.BeginAcceptChannel(timeout, callback, state);
            }

            public IChannelBinder EndAccept(IAsyncResult result)
            {
                IInputChannel channel = this.listener.EndAcceptChannel(result);
                if (channel == null)
                    return null;

                return new InputChannelBinder(channel, this.listener.Uri);
            }
        }

        class InputSessionListenerBinder : IListenerBinder
        {
            IChannelListener<IInputSessionChannel> listener;
            MessageVersion messageVersion;

            internal InputSessionListenerBinder(IChannelListener<IInputSessionChannel> listener, MessageVersion messageVersion)
            {
                this.listener = listener;
                this.messageVersion = messageVersion;
            }

            public IChannelListener Listener
            {
                get { return this.listener; }
            }

            public MessageVersion MessageVersion
            {
                get { return this.messageVersion; }
            }

            public IChannelBinder Accept(TimeSpan timeout)
            {
                IInputSessionChannel channel = this.listener.AcceptChannel(timeout);
                if (null == channel)
                    return null;

                return new InputChannelBinder(channel, this.listener.Uri);
            }

            public IAsyncResult BeginAccept(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.listener.BeginAcceptChannel(timeout, callback, state);
            }

            public IChannelBinder EndAccept(IAsyncResult result)
            {
                IInputSessionChannel channel = this.listener.EndAcceptChannel(result);
                if (channel == null)
                    return null;

                return new InputChannelBinder(channel, this.listener.Uri);
            }
        }

        class ReplyListenerBinder : IListenerBinder
        {
            IChannelListener<IReplyChannel> listener;
            MessageVersion messageVersion;

            internal ReplyListenerBinder(IChannelListener<IReplyChannel> listener, MessageVersion messageVersion)
            {
                this.listener = listener;
                this.messageVersion = messageVersion;
            }

            public IChannelListener Listener
            {
                get { return this.listener; }
            }

            public MessageVersion MessageVersion
            {
                get { return this.messageVersion; }
            }

            public IChannelBinder Accept(TimeSpan timeout)
            {
                IReplyChannel channel = this.listener.AcceptChannel(timeout);
                if (channel == null)
                    return null;

                return new ReplyChannelBinder(channel, listener.Uri);
            }

            public IAsyncResult BeginAccept(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.listener.BeginAcceptChannel(timeout, callback, state);
            }

            public IChannelBinder EndAccept(IAsyncResult result)
            {
                IReplyChannel channel = this.listener.EndAcceptChannel(result);
                if (channel == null)
                    return null;

                return new ReplyChannelBinder(channel, listener.Uri);
            }
        }

        class ReplySessionListenerBinder : IListenerBinder
        {
            IChannelListener<IReplySessionChannel> listener;
            MessageVersion messageVersion;

            internal ReplySessionListenerBinder(IChannelListener<IReplySessionChannel> listener, MessageVersion messageVersion)
            {
                this.listener = listener;
                this.messageVersion = messageVersion;
            }

            public IChannelListener Listener
            {
                get { return this.listener; }
            }

            public MessageVersion MessageVersion
            {
                get { return this.messageVersion; }
            }

            public IChannelBinder Accept(TimeSpan timeout)
            {
                IReplySessionChannel channel = this.listener.AcceptChannel(timeout);
                if (channel == null)
                    return null;

                return new ReplyChannelBinder(channel, listener.Uri);
            }

            public IAsyncResult BeginAccept(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.listener.BeginAcceptChannel(timeout, callback, state);
            }

            public IChannelBinder EndAccept(IAsyncResult result)
            {
                IReplySessionChannel channel = this.listener.EndAcceptChannel(result);
                if (channel == null)
                    return null;

                return new ReplyChannelBinder(channel, listener.Uri);
            }
        }
    }
}
