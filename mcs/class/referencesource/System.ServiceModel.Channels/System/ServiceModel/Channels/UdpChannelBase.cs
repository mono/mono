// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Diagnostics;
    using System.Threading;
    using System.Xml;

    internal abstract class UdpChannelBase<QueueItemType> : InputQueueChannel<QueueItemType>, IUdpReceiveHandler
        where QueueItemType : class, IDisposable
    {
        private bool cleanedUp;
        private long pendingMessagesTotalSize;
        private long maxPendingMessagesTotalSize;
        private int maxReceivedMessageSize;
        private UdpRetransmissionSettings retransmitSettings;
        private Uri via;
        
        protected UdpChannelBase(
            ChannelManagerBase channelManager, 
            MessageEncoder encoder, 
            BufferManager bufferManager,
            UdpSocket[] sockets, 
            UdpRetransmissionSettings retransmissionSettings,
            long maxPendingMessagesTotalSize, 
            EndpointAddress localAddress, 
            Uri via,
            bool isMulticast,
            int maxReceivedMessageSize)
            : base(channelManager)
        {
            Fx.Assert(encoder != null, "encoder shouldn't be null");
            Fx.Assert(bufferManager != null, "buffer manager shouldn't be null");
            Fx.Assert(sockets != null, "sendSockets can't be null");
            Fx.Assert(sockets.Length > 0, "sendSockets can't be empty");
            Fx.Assert(retransmissionSettings != null, "retransmissionSettings can't be null");
            Fx.Assert(maxPendingMessagesTotalSize >= 0, "maxPendingMessagesTotalSize must be >= 0");
            Fx.Assert(maxReceivedMessageSize > 0, "maxReceivedMessageSize must be > 0");
            Fx.Assert(localAddress != null, "localAddress can't be null");
            Fx.Assert(via != null, "via can't be null");

            this.maxPendingMessagesTotalSize = maxPendingMessagesTotalSize == UdpConstants.Defaults.DefaultMaxPendingMessagesTotalSize ? UdpConstants.Defaults.MaxPendingMessagesTotalSize : maxPendingMessagesTotalSize;
            this.Encoder = encoder;
            this.Sockets = sockets;
            this.BufferManager = bufferManager;
            this.retransmitSettings = retransmissionSettings;
            this.IsMulticast = isMulticast;
            this.DuplicateDetector = null;
            this.ReceiveManager = null;
            this.OwnsBufferManager = false;
            this.maxReceivedMessageSize = maxReceivedMessageSize;
            this.LocalAddress = localAddress;
            this.via = via;
        }

        public EndpointAddress LocalAddress
        {
            get;
            private set;
        }

        public Uri Via
        {
            get { return this.via; }
        }

        int IUdpReceiveHandler.MaxReceivedMessageSize
        {
            get { return this.maxReceivedMessageSize; }
        }

        protected abstract bool IgnoreSerializationException { get; }

        protected bool OwnsBufferManager { get; set; }

        protected DuplicateMessageDetector DuplicateDetector { get; set; }

        protected UdpSocketReceiveManager ReceiveManager { get; set; }

        protected BufferManager BufferManager
        {
            get;
            private set;
        }

        protected MessageEncoder Encoder
        {
            get;
            private set;
        }

        protected bool IsMulticast
        {
            get;
            private set;
        }

        protected UdpOutputChannel UdpOutputChannel { get; private set; }

        protected UdpSocket[] Sockets
        {
            get;
            private set;
        }

        [SuppressMessage("Microsoft.StyleCop.CSharp.ReadabilityRules", "SA1100:DoNotPrefixCallsWithBaseUnlessLocalImplementationExists", Justification = "StyleCop 4.5 does not validate this rule properly.")]
        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(IDuplexChannel))
            {
                return (T)(object)this;
            }

            T outputChannelProperty = this.UdpOutputChannel.GetProperty<T>();
            if (outputChannelProperty != null)
            {
                return outputChannelProperty;
            }

            T messageEncoderProperty = this.Encoder.GetProperty<T>();
            if (messageEncoderProperty != null)
            {
                return messageEncoderProperty;
            }

            return base.GetProperty<T>();
        }

        // returns false if the message was dropped because the max pending message count was hit.
        bool IUdpReceiveHandler.HandleDataReceived(ArraySegment<byte> data, EndPoint remoteEndpoint, int interfaceIndex, Action onMessageDequeuedCallback)
        {
            bool returnBuffer = true;
            string messageHash = null;
            Message message = null;
            bool continueReceiving = true;

            try
            {
                IPEndPoint remoteIPEndPoint = (IPEndPoint)remoteEndpoint;

                message = UdpUtility.DecodeMessage(
                    this.DuplicateDetector, 
                    this.Encoder, 
                    this.BufferManager,
                    data, 
                    remoteIPEndPoint, 
                    interfaceIndex, 
                    this.IgnoreSerializationException, 
                    out messageHash);

                if (message != null)
                {
                    // We pass in the length of the message buffer instead of the length of the message to keep track of the amount of memory that's been allocated
                    continueReceiving = this.EnqueueMessage(message, data.Array.Length, onMessageDequeuedCallback);
                    returnBuffer = !continueReceiving;
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    returnBuffer = false;
                    throw;
                }

                this.HandleReceiveException(e);
            }
            finally
            {
                if (returnBuffer)
                {
                    if (message != null)
                    {
                        if (this.DuplicateDetector != null)
                        {
                            Fx.Assert(messageHash != null, "message hash should always be available if duplicate detector is enabled");
                            this.DuplicateDetector.RemoveEntry(messageHash);
                        }

                        message.Close(); // implicitly returns the buffer
                    }
                    else
                    {
                        this.BufferManager.ReturnBuffer(data.Array);
                    }
                }
            }

            return continueReceiving;
        }

        void IUdpReceiveHandler.HandleAsyncException(Exception ex)
        {
            this.HandleReceiveException(ex);
        }

        internal virtual void HandleReceiveException(Exception ex)
        {
            this.EnqueueAndDispatch(UdpUtility.WrapAsyncException(ex), null, false);
        }

        // Since ChannelListener and channel lifetimes can be different, we need a 
        // way to transfer the socketReceiveManager and DuplicateMessageDetection 
        // objects to the channel if the listener gets closed.  If this method succeeds, then 
        // this also indicates that the bufferManager is no longer owned by the channel listener, 
        // so we have to clean that up also.
        internal bool TransferReceiveManagerOwnership(UdpSocketReceiveManager socketReceiveManager, DuplicateMessageDetector duplicateDetector)
        {
            bool success = false;
            if (this.State == CommunicationState.Opened)
            {
                lock (ThisLock)
                {
                    if (this.State == CommunicationState.Opened)
                    {
                        Fx.Assert(this.ReceiveManager == null, "ReceiveManager is already set to a non-null value");
                        Fx.Assert(this.DuplicateDetector == null, "DuplicateDetector is already set to a non-null value");

                        this.ReceiveManager = socketReceiveManager;
                        this.OwnsBufferManager = true;
                        this.ReceiveManager.SetReceiveHandler(this);
                        this.DuplicateDetector = duplicateDetector;
                        success = true;
                    }
                }
            }

            return success;
        }

        // returns false if the max pending messages total size was hit.
        internal bool EnqueueMessage(Message message, int messageBufferSize, Action messageDequeuedCallback)
        {
            Action onMessageDequeuedCallback = () =>
            {
                lock (this.ThisLock)
                {
                    this.pendingMessagesTotalSize -= messageBufferSize;
                    Fx.Assert(this.pendingMessagesTotalSize >= 0, "pendingMessagesTotalSize should not be negative.");
                }

                messageDequeuedCallback();
            };

            bool success = false;
            lock (this.ThisLock)
            {
                if (this.pendingMessagesTotalSize + messageBufferSize <= this.maxPendingMessagesTotalSize)
                {
                    message.Properties.Via = this.Via;
                    this.pendingMessagesTotalSize += messageBufferSize;
                    try
                    {
                        this.FinishEnqueueMessage(message, onMessageDequeuedCallback, false);
                        success = true;
                    }
                    finally
                    {
                        if (!success)
                        {
                            this.pendingMessagesTotalSize -= messageBufferSize;
                        }
                    }
                }
                else
                {
                    if (TD.MaxPendingMessagesTotalSizeReachedIsEnabled())
                    {
                        string messageIdString = string.Empty;
                        if (message.Headers.MessageId != null)
                        {
                            messageIdString = string.Format(CultureInfo.CurrentCulture, "'{0}' ", message.Headers.MessageId.ToString());
                        }

                        EventTraceActivity eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(message);
                        TD.MaxPendingMessagesTotalSizeReached(eventTraceActivity, messageIdString, this.maxPendingMessagesTotalSize, typeof(TransportBindingElement).FullName);
                    }
                }
            }

            return success;
        }

        internal abstract void FinishEnqueueMessage(Message message, Action dequeuedCallback, bool canDispatchOnThisThread);

        protected virtual void AddHeadersTo(Message message)
        {
            Fx.Assert(message != null, "Message can't be null");

            if (message.Version.Addressing != AddressingVersion.None)
            {
                if (message.Headers.MessageId == null)
                {
                    message.Headers.MessageId = new UniqueId();
                }
            }
            else
            {
                if (this.retransmitSettings.Enabled == true)
                {
                    // we should only get here if some channel above us starts producing messages that don't match the encoder's message version.
                    throw FxTrace.Exception.AsError(new ProtocolException(SR.RetransmissionRequiresAddressingOnMessage(message.Version.Addressing.ToString())));
                }
            }
        }

        // Closes the channel ungracefully during error conditions.
        protected override void OnAbort()
        {
            this.Cleanup(true, TimeSpan.Zero);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.OnOpen(timeout);
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            this.UdpOutputChannel.Open();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseAsyncResult<QueueItemType>(
                this, 
                new ChainedBeginHandler(base.OnBeginClose), 
                new ChainedEndHandler(base.OnEndClose),
                timeout, 
                callback, 
                state);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CloseAsyncResult<QueueItemType>.End(result);
        }

        // Closes the channel gracefully during normal conditions.
        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.Cleanup(false, timeoutHelper.RemainingTime());
            base.OnClose(timeoutHelper.RemainingTime());
        }

        protected void SetOutputChannel(UdpOutputChannel udpOutputChannel)
        {
            Fx.Assert(this.UdpOutputChannel == null, "this.UdpOutputChannel must be null");
            Fx.Assert(udpOutputChannel != null, "udpOutputChannel can't be null, since SetOutputChannel should be called only once");

            this.UdpOutputChannel = udpOutputChannel;
        }

        // We're guaranteed by CommunicationObject that at most ONE of Close or BeginClose will be called once. 
        protected void Cleanup(bool aborting, TimeSpan timeout)
        {           
            if (this.cleanedUp)
            {
                return;
            }
                
            lock (ThisLock)
            {
                if (this.cleanedUp)
                {
                    return;
                }

                if (aborting)
                {
                    this.UdpOutputChannel.Abort();
                }
                else
                {
                    this.UdpOutputChannel.Close(timeout);
                }
               
                if (this.DuplicateDetector != null)
                {
                    this.DuplicateDetector.Dispose();
                }

                if (this.ReceiveManager != null)
                {
                    this.ReceiveManager.Close();
                }

                this.CleanupBufferManager();

                this.cleanedUp = true;
            }
        }

        private void CleanupBufferManager()
        {
            if (this.OwnsBufferManager)
            {
                this.BufferManager.Clear();
            }
        }

        // Control flow for async path
        // We use this mechanism to avoid initializing two async objects as logically cleanup+close is one operation. 
        // At any point in the Begin* methods, we may go async. The steps are: 
        // - Close inner UdpOutputChannel
        // - Cleanup channel
        // - Close channel
        private class CloseAsyncResult<T> : AsyncResult
            where T : class, IDisposable
        {
            private static AsyncCompletion completeCloseOutputChannelCallback = new AsyncCompletion(CompleteCloseOutputChannel);
            private static AsyncCompletion completeBaseCloseCallback = new AsyncCompletion(CompleteBaseClose);

            private UdpChannelBase<T> channel;
            private TimeoutHelper timeoutHelper;
            private ChainedBeginHandler baseBeginClose;
            private ChainedEndHandler baseEndClose;

            public CloseAsyncResult(UdpChannelBase<T> channel, ChainedBeginHandler baseBeginClose, ChainedEndHandler baseEndClose, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.channel = channel;
                this.baseBeginClose = baseBeginClose;
                this.baseEndClose = baseEndClose;
                this.timeoutHelper = new TimeoutHelper(timeout);

                if (this.BeginCloseOutputChannel())
                {
                    this.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<CloseAsyncResult<T>>(result);
            }

            private static bool CompleteBaseClose(IAsyncResult result)
            {
                // AsyncResult.AsyncCompletionWrapperCallback takes care of catching exceptions for us. 
                CloseAsyncResult<T> thisPtr = (CloseAsyncResult<T>)result.AsyncState;

                // We are completing the base class close operation at this point.
                thisPtr.baseEndClose(result);

                return true;
            }

            private static bool CompleteCloseOutputChannel(IAsyncResult result)
            {
                // AsyncResult.AsyncCompletionWrapperCallback takes care of catching exceptions for us. 
                CloseAsyncResult<T> thisPtr = (CloseAsyncResult<T>)result.AsyncState;

                // We are completing the base class close operation at this point.
                thisPtr.channel.UdpOutputChannel.EndClose(result);

                thisPtr.channel.Cleanup(false, thisPtr.timeoutHelper.RemainingTime());

                return thisPtr.BeginBaseClose();
            }

            private bool BeginCloseOutputChannel()
            {
                // AsyncResult.AsyncCompletionWrapperCallback takes care of catching the exceptions for us. 
                IAsyncResult result = this.channel.UdpOutputChannel.BeginClose(this.timeoutHelper.RemainingTime(), this.PrepareAsyncCompletion(completeCloseOutputChannelCallback), this);
                
                // SyncContinue calls CompleteCloseOutputChannel for us in [....] case. 
                return this.SyncContinue(result);
            }

            private bool BeginBaseClose()
            {
                // AsyncResult.AsyncCompletionWrapperCallback takes care of catching the exceptions for us. 
                IAsyncResult result = this.baseBeginClose(this.timeoutHelper.RemainingTime(), this.PrepareAsyncCompletion(completeBaseCloseCallback), this);
                
                // SyncContinue calls CompleteBaseClose for us in [....] case. 
                return this.SyncContinue(result);
            }
        }
    }
}
