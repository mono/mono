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

    internal abstract class UdpOutputChannel : OutputChannel, IOutputChannel
    {
        private bool cleanedUp;
        private volatile AsyncWaitHandle retransmissionDoneWaitHandle;
        private UdpRetransmissionSettings retransmitSettings;
        private volatile Dictionary<UniqueId, IUdpRetransmitter> retransmitList;
        private SynchronizedRandom randomNumberGenerator;
        private Uri via;

        public UdpOutputChannel(
            ChannelManagerBase factory,
            MessageEncoder encoder,
            BufferManager bufferManager,
            UdpSocket[] sendSockets,
            UdpRetransmissionSettings retransmissionSettings,
            Uri via,
            bool isMulticast)
            : base(factory)
        {
            Fx.Assert(encoder != null, "encoder shouldn't be null");
            Fx.Assert(bufferManager != null, "buffer manager shouldn't be null");
            Fx.Assert(sendSockets != null, "sendSockets can't be null");
            Fx.Assert(sendSockets.Length > 0, "sendSockets can't be empty");
            Fx.Assert(retransmissionSettings != null, "retransmissionSettings can't be null");
            Fx.Assert(via != null, "via can't be null");

            this.BufferManager = bufferManager;
            this.IsMulticast = isMulticast;
            this.Encoder = encoder;
            this.retransmitSettings = retransmissionSettings;
            this.SendSockets = sendSockets;
            this.via = via;

            if (this.retransmitSettings.Enabled)
            {
                this.retransmitList = new Dictionary<UniqueId, IUdpRetransmitter>();
                this.randomNumberGenerator = new SynchronizedRandom(AppDomain.CurrentDomain.GetHashCode() | Environment.TickCount);
            }
        }

        private interface IUdpRetransmitter
        {
            bool IsMulticast { get; }

            void CancelRetransmission();
        }

        public override EndpointAddress RemoteAddress
        {
            get { return null; }
        }

        public override Uri Via
        {
            get { return this.via; }
        }

        internal bool IsMulticast
        {
            get;
            private set;
        }

        internal TimeSpan InternalSendTimeout
        {
            get { return this.DefaultSendTimeout; }
        }

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

        protected UdpSocket[] SendSockets
        {
            get;
            private set;
        }

        [SuppressMessage("Microsoft.StyleCop.CSharp.ReadabilityRules", "SA1100:DoNotPrefixCallsWithBaseUnlessLocalImplementationExists", Justification = "StyleCop 4.5 does not validate this rule properly.")]
        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(IOutputChannel))
            {
                return (T)(object)this;
            }

            T messageEncoderProperty = this.Encoder.GetProperty<T>();
            if (messageEncoderProperty != null)
            {
                return messageEncoderProperty;
            }

            return base.GetProperty<T>();
        }

        internal void CancelRetransmission(UniqueId messageId)
        {
            if (messageId != null && this.retransmitList != null)
            {
                lock (this.ThisLock)
                {
                    if (this.retransmitList != null)
                    {
                        IUdpRetransmitter retransmitter;
                        if (this.retransmitList.TryGetValue(messageId, out retransmitter))
                        {
                            this.retransmitList.Remove(messageId);
                            retransmitter.CancelRetransmission();
                        }
                    }
                }
            }
        }

        protected static void LogMessage(ref Message message, ArraySegment<byte> messageData)
        {
            using (XmlDictionaryReader xmlDictionaryReader = XmlDictionaryReader.CreateTextReader(messageData.Array, messageData.Offset, messageData.Count, null, XmlDictionaryReaderQuotas.Max, null))
            {
                MessageLogger.LogMessage(ref message, xmlDictionaryReader, MessageLoggingSource.TransportSend);
            }
        }

        protected override void AddHeadersTo(Message message)
        {
            Fx.Assert(message != null, "Message can't be null");
            
            if (message is NullMessage)
            {
                return; 
            }

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

        protected override IAsyncResult OnBeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (message is NullMessage)
            {
                return new CompletedAsyncResult(callback, state); 
            }
            
            return new SendAsyncResult(this, message, timeout, callback, state);
        }

        protected override void OnEndSend(IAsyncResult result)
        {
            if (result is CompletedAsyncResult)
            {
                CompletedAsyncResult.End(result); 
            }
            else 
            {
                SendAsyncResult.End(result);
            }
        }

        protected override void OnSend(Message message, TimeSpan timeout)
        {
            if (message is NullMessage)
            {
                return; 
            }
        
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

            IPEndPoint remoteEndPoint;
            UdpSocket[] sendSockets;
            Exception exceptionToBeThrown;
            sendSockets = this.GetSendSockets(message, out remoteEndPoint, out exceptionToBeThrown);

            if (exceptionToBeThrown != null)
            {
                throw FxTrace.Exception.AsError(exceptionToBeThrown);
            }

            if (timeoutHelper.RemainingTime() <= TimeSpan.Zero)
            {
                throw FxTrace.Exception.AsError(new TimeoutException(SR.SendTimedOut(remoteEndPoint, timeout)));
            }

            bool returnBuffer = false;
            ArraySegment<byte> messageData = default(ArraySegment<byte>);

            bool sendingMulticast = UdpUtility.IsMulticastAddress(remoteEndPoint.Address);
            SynchronousRetransmissionHelper retransmitHelper = null;
            RetransmitIterator retransmitIterator = null;

            bool shouldRetransmit = this.ShouldRetransmitMessage(sendingMulticast);
            
            try
            {
                if (shouldRetransmit)
                {
                    retransmitIterator = this.CreateRetransmitIterator(sendingMulticast);
                    retransmitHelper = new SynchronousRetransmissionHelper(sendingMulticast);
                    this.RetransmitStarting(message.Headers.MessageId, retransmitHelper);
                }

                messageData = this.EncodeMessage(message);
                returnBuffer = true;

                this.TransmitMessage(messageData, sendSockets, remoteEndPoint, timeoutHelper);

                if (shouldRetransmit)
                {
                    while (retransmitIterator.MoveNext())
                    {
                        // wait for currentDelay time, then retransmit
                        if (retransmitIterator.CurrentDelay > 0)
                        {
                            retransmitHelper.Wait(retransmitIterator.CurrentDelay);
                        }

                        if (retransmitHelper.IsCanceled)
                        {
                            ThrowIfAborted();
                            return;
                        }

                        // since we only invoke the encoder once just before the initial send of the message
                        // we need to handle logging the message in the retransmission case
                        if (MessageLogger.LogMessagesAtTransportLevel)
                        {
                            UdpOutputChannel.LogMessage(ref message, messageData);
                        }

                        this.TransmitMessage(messageData, sendSockets, remoteEndPoint, timeoutHelper);
                    }
                }
            }
            finally
            {
                if (returnBuffer)
                {
                    this.BufferManager.ReturnBuffer(messageData.Array);
                }

                if (shouldRetransmit)
                {
                    this.RetransmitStopping(message.Headers.MessageId);

                    if (retransmitHelper != null)
                    {
                        retransmitHelper.Dispose();
                    }
                }
            }
        }

        protected abstract UdpSocket[] GetSendSockets(Message message, out IPEndPoint remoteEndPoint, out Exception exceptionToBeThrown);

        protected override void OnAbort()
        {
            this.Cleanup(true, TimeSpan.Zero);
        }

        [SuppressMessage("Microsoft.StyleCop.CSharp.ReadabilityRules", "SA1100:DoNotPrefixCallsWithBaseUnlessLocalImplementationExists", Justification = "If BeginClose is overridden we still pass base.BeginClose here")]
        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseAsyncResult(
                this,
                timeout,
                callback,
                state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.Cleanup(false, timeoutHelper.RemainingTime());
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CloseAsyncResult.End(result);
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
            for (int i = 0; i < this.SendSockets.Length; i++)
            {
                this.SendSockets[i].Open();
            }
        }

        protected ArraySegment<byte> EncodeMessage(Message message)
        {
            return this.Encoder.WriteMessage(message, int.MaxValue, this.BufferManager);
        }

        protected ObjectDisposedException CreateObjectDisposedException()
        {
            return new ObjectDisposedException(null, SR.ObjectDisposed(this.GetType().Name));
        }

        private RetransmitIterator CreateRetransmitIterator(bool sendingMulticast)
        {
            Fx.Assert(this.retransmitSettings.Enabled, "CreateRetransmitCalculator called when no retransmission set to happen");
            int lowerBound = this.retransmitSettings.GetDelayLowerBound();
            int upperBound = this.retransmitSettings.GetDelayUpperBound();
            int currentDelay = this.randomNumberGenerator.Next(lowerBound, upperBound);

            int maxDelay = this.retransmitSettings.GetMaxDelayPerRetransmission();
            int maxRetransmitCount = sendingMulticast ? this.retransmitSettings.MaxMulticastRetransmitCount : this.retransmitSettings.MaxUnicastRetransmitCount;

            return new RetransmitIterator(currentDelay, maxDelay, maxRetransmitCount);
        }

        private void RetransmitStarting(UniqueId messageId, IUdpRetransmitter retransmitter)
        {
            Fx.Assert(this.retransmitSettings.Enabled, "RetransmitStarting called when retransmission is disabled");

            lock (this.ThisLock)
            {
                ThrowIfDisposed();

                if (this.retransmitList.ContainsKey(messageId))
                {
                    // someone is sending a message with the same MessageId 
                    // while a retransmission is still in progress for that ID.
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.RecycledMessageIdDuringRetransmission(messageId)));
                }
                else
                {
                    this.retransmitList[messageId] = retransmitter;
                }
            }
        }

        private void RetransmitStopping(UniqueId messageId)
        {
            Fx.Assert(this.retransmitSettings.Enabled, "RetransmitStopping called when retransmission is disabled");

            lock (this.ThisLock)
            {
                // Cleanup sets retransmitList to null, so check before using...
                if (this.retransmitList != null)
                {
                    this.retransmitList.Remove(messageId);

                    // if we are closing down, then we need to unblock the Cleanup code 
                    //  this.retransmissionDoneEvent only != null if on cleaning up; abort case means that it == null. 
                    if (this.retransmitList.Count == 0 && this.retransmissionDoneWaitHandle != null)
                    {
                        this.retransmissionDoneWaitHandle.Set();
                    }
                }
            }
        }

        private bool ShouldRetransmitMessage(bool sendingMulticast)
        {
            if (sendingMulticast)
            {
                return this.retransmitSettings.MaxMulticastRetransmitCount > 0;
            }
            else
            {
                return this.retransmitSettings.MaxUnicastRetransmitCount > 0;
            }
        }

        private void TransmitMessage(ArraySegment<byte> messageBytes, UdpSocket[] sockets, IPEndPoint remoteEndpoint, TimeoutHelper timeoutHelper)
        {
            Fx.Assert(messageBytes.Array != null, "message data array can't be null");
            Fx.Assert(sockets != null, "sockets can't be null");
            Fx.Assert(sockets.Length > 0, "sockets must contain at least one item");
            Fx.Assert(remoteEndpoint != null, "remoteEndPoint can't be null");

            for (int i = 0; i < sockets.Length; i++)
            {
                if (timeoutHelper.RemainingTime() <= TimeSpan.Zero)
                {
                    throw FxTrace.Exception.AsError(new TimeoutException(SR.SendTimedOut(remoteEndpoint, timeoutHelper.OriginalTimeout)));
                }

                sockets[i].SendTo(messageBytes.Array, messageBytes.Offset, messageBytes.Count, remoteEndpoint);
            }
        }

        // we're guaranteed by CommunicationObject that at most ONE of Close or BeginClose will be called once. 
        private void Cleanup(bool aborting, TimeSpan timeout)
        {
            bool needToWait = false;

            if (this.cleanedUp)
            {
                return;
            }

            lock (this.ThisLock)
            {
                if (this.cleanedUp)
                {
                    return;
                }

                if (!aborting && this.retransmitList != null && this.retransmitList.Count > 0)
                {
                    needToWait = true;
                    this.retransmissionDoneWaitHandle = new AsyncWaitHandle(EventResetMode.ManualReset);
                }
                else
                {
                    // copied this call here in order to avoid releasing then retaking lock 
                    this.CleanupAfterWait(aborting);
                }
            }

            if (needToWait)
            {
                if (!this.retransmissionDoneWaitHandle.Wait(timeout))
                {
                    throw FxTrace.Exception.AsError(new TimeoutException(SR.TimeoutOnOperation(timeout)));
                }

                lock (this.ThisLock)
                {
                    this.retransmissionDoneWaitHandle = null;

                    // another thread could have called Abort while Close() was waiting for retransmission to complete.
                    if (this.cleanedUp)
                    {
                        return;
                    }

                    this.CleanupAfterWait(aborting);
                }
            }
        }

        // must be called from within this.ThisLock
        private void CleanupAfterWait(bool aborting)
        {
            Fx.Assert(!this.cleanedUp, "We should only clean up once");

            if (this.retransmitList != null)
            {
                foreach (IUdpRetransmitter retransmitter in this.retransmitList.Values)
                {
                    retransmitter.CancelRetransmission();
                }

                if (aborting && this.retransmissionDoneWaitHandle != null)
                {
                    // If another thread has called close and is waiting for retransmission to complete,
                    // we need to make sure that thread gets unblocked.
                    this.retransmissionDoneWaitHandle.Set();
                }

                this.retransmitList = null;
            }

            for (int i = 0; i < this.SendSockets.Length; i++)
            {
                this.SendSockets[i].Close();
            }                

            this.cleanedUp = true;
        }

        private class RetransmitIterator
        {
            private int maxDelay;
            private int retransmitCount;
            private int initialDelay;

            internal RetransmitIterator(int initialDelay, int maxDelay, int retransmitCount)
            {
                Fx.Assert(initialDelay >= 0, "initialDelay cannot be negative");
                Fx.Assert(maxDelay >= initialDelay, "maxDelay must be >= initialDelay");
                Fx.Assert(retransmitCount > 0, "retransmitCount must be > 0");

                this.CurrentDelay = -1;
                this.initialDelay = initialDelay;
                this.maxDelay = maxDelay;
                this.retransmitCount = retransmitCount;
            }

            public int CurrentDelay
            {
                get;
                private set;
            }

            // should be called before each retransmission to determine if 
            // another one is needed.
            public bool MoveNext()
            {
                if (this.CurrentDelay < 0)
                {
                    this.CurrentDelay = this.initialDelay;
                    return true;
                }

                bool shouldContinue = --this.retransmitCount > 0;

                if (shouldContinue && this.CurrentDelay < this.maxDelay)
                {
                    this.CurrentDelay = Math.Min(this.CurrentDelay * 2, this.maxDelay);
                }

                return shouldContinue;
            }
        }

        private sealed class SynchronousRetransmissionHelper : IUdpRetransmitter, IDisposable
        {
            private ManualResetEvent cancelEvent;
            private object thisLock;
            private bool cleanedUp;

            public SynchronousRetransmissionHelper(bool isMulticast)
            {
                this.thisLock = new object();
                this.IsMulticast = isMulticast;
                this.cancelEvent = new ManualResetEvent(false);
            }

            public bool IsMulticast
            {
                get;
                private set;
            }

            public bool IsCanceled
            {
                get;
                private set;
            }

            public void Wait(int millisecondsTimeout)
            {
                if (this.ResetEvent())
                {
                    // Dispose should only be called by the same thread that
                    // is calling this function, making it so that we don't need a lock here...
                    this.cancelEvent.WaitOne(millisecondsTimeout);
                }
            }

            public void CancelRetransmission()
            {
                lock (this.thisLock)
                {
                    this.IsCanceled = true;

                    if (!this.cleanedUp)
                    {
                        this.cancelEvent.Set();
                    }
                }
            }

            public void Dispose()
            {
                lock (this.thisLock)
                {
                    if (!this.cleanedUp)
                    {
                        this.cleanedUp = true;
                        this.cancelEvent.Dispose();
                    }
                }
            }

            private bool ResetEvent()
            {
                lock (this.thisLock)
                {
                    if (!this.IsCanceled && !this.cleanedUp)
                    {
                        this.cancelEvent.Reset();
                        return true;
                    }
                }

                return false;
            }
        }

        private class SendAsyncResult : AsyncResult, IUdpRetransmitter
        {
            private static AsyncCallback onSocketSendComplete = Fx.ThunkCallback(new AsyncCallback(OnSocketSendComplete));
            private static Action<object> onRetransmitMessage = new Action<object>(OnRetransmitMessage);

            private UdpOutputChannel channel;
            private ArraySegment<byte> messageData;
            private TimeoutHelper timeoutHelper;
            private IPEndPoint remoteEndpoint;
            private int currentSocket;
            private UdpSocket[] sendSockets;
            private IOThreadTimer retransmitTimer;
            private RetransmitIterator retransmitIterator;
            private Message message;
            private bool retransmissionEnabled;

            public SendAsyncResult(UdpOutputChannel channel, Message message, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.timeoutHelper = new TimeoutHelper(timeout);

                this.channel = channel;
                this.message = message;
                bool throwing = true;
                bool completedSynchronously = false;

                try
                {
                    this.Initialize(message);

                    completedSynchronously = this.BeginTransmitMessage();

                    if (completedSynchronously && this.retransmissionEnabled)
                    {
                        // initial send completed [....], now we need to start the retransmission process...
                        completedSynchronously = this.BeginRetransmission();
                    }

                    throwing = false;
                }
                finally
                {
                    if (throwing)
                    {
                        this.Cleanup();
                    }
                }

                if (completedSynchronously)
                {
                    this.CompleteAndCleanup(true, null);
                }
            }

            private enum RetransmitState
            {
                WaitCompleted,
                TransmitCompleted,
            }

            public bool IsCanceled
            {
                get;
                private set;
            }

            public bool IsMulticast
            {
                get;
                private set;
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<SendAsyncResult>(result);
            }

            // tries to terminate retransmission early, but won't cancel async IO immediately
            public void CancelRetransmission()
            {
                this.IsCanceled = true;
            }

            private static void OnSocketSendComplete(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                SendAsyncResult thisPtr = (SendAsyncResult)result.AsyncState;
                bool completeSelf = false;
                Exception completionException = null;

                try
                {
                    completeSelf = thisPtr.ContinueTransmitting(result);

                    if (completeSelf && thisPtr.retransmissionEnabled)
                    {
                        completeSelf = thisPtr.BeginRetransmission();
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completionException = e;
                    completeSelf = true;
                }

                if (completeSelf)
                {
                    thisPtr.CompleteAndCleanup(false, completionException);
                }
            }

            private static void OnRetransmitMessage(object state)
            {
                SendAsyncResult thisPtr = (SendAsyncResult)state;
                bool completeSelf = false;
                Exception completionException = null;

                try
                {
                    completeSelf = thisPtr.ContinueRetransmission(RetransmitState.WaitCompleted);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completionException = e;
                    completeSelf = true;
                }

                if (completeSelf)
                {
                    thisPtr.CompleteAndCleanup(false, completionException);
                }
            }

            private bool BeginTransmitMessage()
            {
                this.currentSocket = 0;
                return this.ContinueTransmitting(null);
            }

            private bool ContinueTransmitting(IAsyncResult socketAsyncResult)
            {
                while (this.currentSocket < this.sendSockets.Length)
                {
                    if (socketAsyncResult == null)
                    {
                        socketAsyncResult = this.sendSockets[this.currentSocket].BeginSendTo(
                            this.messageData.Array,
                            this.messageData.Offset,
                            this.messageData.Count,
                            this.remoteEndpoint,
                            onSocketSendComplete,
                            this);

                        if (!socketAsyncResult.CompletedSynchronously)
                        {
                            return false;
                        }
                    }

                    this.sendSockets[this.currentSocket].EndSendTo(socketAsyncResult);

                    // check for timeout after calling socket.EndSendTo 
                    // so that we don't leave the socket in a bad state/leak async results
                    this.ThrowIfTimedOut();

                    if (this.IsCanceled)
                    {
                        // don't send on the next socket and return true to cause Complete to be called.
                        return true;
                    }

                    this.currentSocket++;
                    socketAsyncResult = null;
                }

                return true;
            }

            private bool BeginRetransmission()
            {
                // BeginRetransmission should only be called in the case where transmission of the message
                // completes synchronously.
                return this.ContinueRetransmission(RetransmitState.TransmitCompleted);
            }

            private bool ContinueRetransmission(RetransmitState state)
            {
                this.ThrowIfTimedOut();

                while (true)
                {
                    switch (state)
                    {
                        case RetransmitState.TransmitCompleted:
                            if (!this.retransmitIterator.MoveNext())
                            {
                                // We are done retransmitting
                                return true;
                            }

                            if (this.retransmitIterator.CurrentDelay > 0)
                            {
                                this.retransmitTimer.Set(this.retransmitIterator.CurrentDelay);
                                return false;
                            }

                            state = RetransmitState.WaitCompleted;
                            break;
                        case RetransmitState.WaitCompleted:
                            if (this.IsCanceled)
                            {
                                this.channel.ThrowIfAborted();
                                return true;
                            }

                            // since we only invoke the encoder once just before the initial send of the message
                            // we need to handle logging the message in the retransmission case
                            if (MessageLogger.LogMessagesAtTransportLevel)
                            {
                                UdpOutputChannel.LogMessage(ref this.message, this.messageData);
                            }

                            // !completedSync
                            if (!this.BeginTransmitMessage())
                            {
                                return false;
                            }

                            state = RetransmitState.TransmitCompleted;
                            break;

                        default:
                            Fx.Assert("Unknown RetransmitState value encountered");
                            return true;
                    }
                }
            }

            private void Initialize(Message message)
            {
                Exception exceptionToThrow;
                this.sendSockets = this.channel.GetSendSockets(message, out this.remoteEndpoint, out exceptionToThrow);

                if (exceptionToThrow != null)
                {
                    throw FxTrace.Exception.AsError(exceptionToThrow);
                }

                this.IsMulticast = UdpUtility.IsMulticastAddress(this.remoteEndpoint.Address);

                if (this.channel.ShouldRetransmitMessage(this.IsMulticast))
                {
                    this.retransmissionEnabled = true;
                    this.channel.RetransmitStarting(this.message.Headers.MessageId, this);
                    this.retransmitTimer = new IOThreadTimer(onRetransmitMessage, this, false);
                    this.retransmitIterator = this.channel.CreateRetransmitIterator(this.IsMulticast);
                }

                this.messageData = this.channel.EncodeMessage(message);
            }

            private void ThrowIfTimedOut()
            {
                if (this.timeoutHelper.RemainingTime() <= TimeSpan.Zero)
                {
                    throw FxTrace.Exception.AsError(new TimeoutException(SR.TimeoutOnOperation(this.timeoutHelper.OriginalTimeout)));
                }
            }

            private void Cleanup()
            {
                if (this.retransmissionEnabled)
                {
                    this.channel.RetransmitStopping(this.message.Headers.MessageId);
                    this.retransmitTimer.Cancel();
                }

                if (this.messageData.Array != null)
                {
                    this.channel.BufferManager.ReturnBuffer(this.messageData.Array);
                    this.messageData = default(ArraySegment<byte>);
                }
            }

            private void CompleteAndCleanup(bool completedSynchronously, Exception completionException)
            {
                this.Cleanup();
                this.Complete(completedSynchronously, completionException);
            }
        }

        // Control flow for async path
        // We use this mechanism to avoid initializing two async objects as logically cleanup+close is one operation. 
        // At any point in the Begin* methods, we may go async. The steps are: 
        // - Cleanup channel
        // - Close channel
        private class CloseAsyncResult : AsyncResult
        {
            private static Action<object, TimeoutException> completeCleanupCallback = new Action<object, TimeoutException>(CompleteCleanup);

            private UdpOutputChannel channel;
            private TimeoutHelper timeoutHelper;

            public CloseAsyncResult(UdpOutputChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.channel = channel;
                this.timeoutHelper = new TimeoutHelper(timeout);

                if (this.BeginCleanup())
                {
                    this.Complete(true);
                }
            }

            internal static void End(IAsyncResult result)
            {
                AsyncResult.End<CloseAsyncResult>(result);
            }

            private static void CompleteCleanup(object state, TimeoutException exception)
            {
                CloseAsyncResult thisPtr = (CloseAsyncResult)state;
                Exception completionException = null;

                if (exception != null)
                {
                    Fx.Assert(exception.GetType() == typeof(TimeoutException), "Exception on callback should always be TimeoutException");
                    throw FxTrace.Exception.AsError(new TimeoutException(SR.TimeoutOnOperation(thisPtr.timeoutHelper.OriginalTimeout)));
                }

                try
                {
                    lock (thisPtr.channel.ThisLock)
                    {
                        thisPtr.channel.retransmissionDoneWaitHandle = null;

                        // another thread could have called Abort while Close() was waiting for retransmission to complete.
                        if (!thisPtr.channel.cleanedUp)
                        {
                            // never aborting here
                            thisPtr.channel.CleanupAfterWait(false);
                        }
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completionException = e;
                }

                thisPtr.Complete(false, completionException);
            }

            private bool BeginCleanup()
            {
                bool needToWait = false;

                if (this.channel.cleanedUp)
                {
                    return true;
                }

                lock (this.channel.ThisLock)
                {
                    if (this.channel.cleanedUp)
                    {
                        return true;
                    }

                    // we're never aborting in this case...
                    if (this.channel.retransmitList != null && this.channel.retransmitList.Count > 0)
                    {
                        needToWait = true;
                        this.channel.retransmissionDoneWaitHandle = new AsyncWaitHandle(EventResetMode.ManualReset);
                    }
                    else
                    {
                        this.channel.CleanupAfterWait(false);
                    }

                    // we're guaranteed by CommunicationObject that at most ONE of Close or BeginClose will be called once. 
                    // we don't null out retransmissionDoneEvent in the abort case; should be safe to use here. 
                    return !needToWait || this.channel.retransmissionDoneWaitHandle.WaitAsync(completeCleanupCallback, this, this.timeoutHelper.RemainingTime());
                }
            }
        }
    }
}
