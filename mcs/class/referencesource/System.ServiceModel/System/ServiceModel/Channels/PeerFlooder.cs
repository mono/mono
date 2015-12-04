//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;
    using System.Threading;
    using System.Xml;

    class PeerFlooder : PeerFlooderSimple
    {
        PeerFlooder(PeerNodeConfig config, PeerNeighborManager neighborManager) : base(config, neighborManager) { }

        public static PeerFlooder CreateFlooder(PeerNodeConfig config, PeerNeighborManager neighborManager, IPeerNodeMessageHandling messageHandler)
        {
            PeerFlooder flooder = new PeerFlooder(config, neighborManager);
            flooder.messageHandler = messageHandler;
            return flooder;
        }
    }

    interface IFlooderForThrottle
    {
        void OnThrottleReached();
        void OnThrottleReleased();
    }

    abstract class PeerFlooderBase<TFloodContract, TLinkContract> : IFlooderForThrottle, IPeerFlooderContract<TFloodContract, TLinkContract> where TFloodContract : Message
    {
        protected PeerNodeConfig config;
        protected PeerNeighborManager neighborManager;
        protected List<IPeerNeighbor> neighbors;
        object thisLock = new object();

        internal IPeerNodeMessageHandling messageHandler;
        internal PeerThrottleHelper quotaHelper;
        long messageSequence;

        public event EventHandler ThrottleReached;
        public event EventHandler SlowNeighborKilled;
        public event EventHandler ThrottleReleased;
        public EventHandler OnMessageSentHandler;


        public PeerFlooderBase(PeerNodeConfig config, PeerNeighborManager neighborManager)
        {
            this.neighborManager = neighborManager;
            this.neighbors = new List<IPeerNeighbor>();
            this.config = config;
            this.neighbors = this.neighborManager.GetConnectedNeighbors();
            this.quotaHelper = new PeerThrottleHelper(this, this.config.MaxPendingOutgoingCalls);
            OnMessageSentHandler = new EventHandler(OnMessageSent);
        }

        void PruneNeighborCallback(IPeerNeighbor peer)
        {
            lock (ThisLock)
            {
                if (this.Neighbors.Count <= 1)
                    return;
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    string message = SR.GetString(SR.PeerThrottlePruning, this.config.MeshId);
                    PeerThrottleTraceRecord record = new PeerThrottleTraceRecord(this.config.MeshId, message);
                    TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.PeerFlooderReceiveMessageQuotaExceeded,
                        SR.GetString(SR.TraceCodePeerFlooderReceiveMessageQuotaExceeded), record, this, null);
                }
            }
            try
            {
                peer.Abort(PeerCloseReason.NodeTooSlow, PeerCloseInitiator.LocalNode);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e)) throw;
                if (null != CloseNeighborIfKnownException(neighborManager, e, peer))
                {
                    throw;
                }
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
        }

        void IFlooderForThrottle.OnThrottleReached()
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                string message = SR.GetString(SR.PeerThrottleWaiting, this.config.MeshId);
                PeerThrottleTraceRecord record = new PeerThrottleTraceRecord(this.config.MeshId, message);
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.PeerFlooderReceiveMessageQuotaExceeded,
                    SR.GetString(SR.TraceCodePeerFlooderReceiveMessageQuotaExceeded), record, this, null);
            }

            IPeerNeighbor peer = this.neighborManager.SlowestNeighbor();
            if (peer == null)
                return;
            UtilityExtension extension = peer.Utility;
            if (peer.IsConnected && extension != null)
            {
                if (extension.PendingMessages > PeerTransportConstants.MessageThreshold)
                {
                    extension.BeginCheckPoint(new UtilityExtension.PruneNeighborCallback(PruneNeighborCallback));
                }
                else
                {
                    Fx.Assert(false, "Neighbor is marked slow with messages " + extension.PendingMessages);
                }
                FireReachedEvent();
            }
        }

        void IFlooderForThrottle.OnThrottleReleased()
        {
            FireDequeuedEvent();
        }

        public void FireDequeuedEvent() { FireEvent(ThrottleReleased); }

        public void FireReachedEvent() { FireEvent(ThrottleReached); }

        public void FireKilledEvent() { FireEvent(SlowNeighborKilled); }

        void FireEvent(EventHandler handler)
        {
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        public virtual IAsyncResult BeginFloodEncodedMessage(byte[] id, MessageBuffer encodedMessage, TimeSpan timeout, AsyncCallback callback, object state)
        {
            RecordOutgoingMessage(id);
            SynchronizationContext syncContext = ThreadBehavior.GetCurrentSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(null);

            if (neighbors.Count == 0)
            {
                return new CompletedAsyncResult(callback, state);
            }
            try
            {
                return FloodMessageToNeighbors(encodedMessage, timeout, callback, state, -1, null, null, OnMessageSentHandler);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(syncContext);
            }

        }

        protected virtual IAsyncResult BeginFloodReceivedMessage(IPeerNeighbor sender, MessageBuffer messageBuffer,
            TimeSpan timeout, AsyncCallback callback, object state, int index, MessageHeader hopHeader)
        {
            quotaHelper.AcquireNoQueue();

            try
            {
                return FloodMessageToNeighbors(messageBuffer, timeout, callback, state, index, hopHeader, sender, OnMessageSentHandler);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e)) throw;
                if (e is QuotaExceededException || (e is CommunicationException && e.InnerException is QuotaExceededException))
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    if (DiagnosticUtility.ShouldTraceError)
                    {
                        PeerFlooderTraceRecord record = new PeerFlooderTraceRecord(
                                                            this.config.MeshId,
                                                            sender.ListenAddress,
                                                            e);
                        TraceUtility.TraceEvent(
                                    TraceEventType.Error,
                                    TraceCode.PeerFlooderReceiveMessageQuotaExceeded,
                                    SR.GetString(SR.TraceCodePeerFlooderReceiveMessageQuotaExceeded),
                                    record,
                                    this,
                                    null);
                    }
                    return null;
                }
                throw;
            }
        }

        protected IAsyncResult BeginSendHelper(IPeerNeighbor neighbor, TimeSpan timeout, Message message, FloodAsyncResult fresult)
        {
            IAsyncResult result = null;
            bool fatal = false;
            try
            {
                UtilityExtension.OnMessageSent(neighbor);
                result = neighbor.BeginSend(message, timeout, Fx.ThunkCallback(new AsyncCallback(fresult.OnSendComplete)), message);
                fresult.AddResult(result, neighbor);
                if (result.CompletedSynchronously)
                {
                    neighbor.EndSend(result);
                    UtilityExtension.OnEndSend(neighbor, fresult);
                }
                return result;
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    fatal = true;
                    throw;
                }
                if (null != CloseNeighborIfKnownException(neighborManager, e, neighbor))
                {
                    fresult.MarkEnd(false);
                    throw;
                }

                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                return null;
            }
            finally
            {
                if ((result == null || result.CompletedSynchronously) && !fatal)
                    message.Close();

            }
        }

        public void OnMessageSent(object sender, EventArgs args)
        {
            quotaHelper.ItemDequeued();
        }

        void KillSlowNeighbor()
        {
            IPeerNeighbor neighbor = this.neighborManager.SlowestNeighbor();
            if (neighbor != null)
                neighbor.Abort(PeerCloseReason.NodeTooSlow, PeerCloseInitiator.LocalNode);
        }


        protected virtual IAsyncResult FloodMessageToNeighbors(MessageBuffer messageBuffer,
                                                               TimeSpan timeout, AsyncCallback callback, object state,
                                                               int index, MessageHeader hopHeader, IPeerNeighbor except,
                                                               EventHandler OnMessageSentCallback)
        {
            long temp = Interlocked.Increment(ref messageSequence);
            FloodAsyncResult fresult = new FloodAsyncResult(this.neighborManager, timeout, callback, state);
            fresult.OnMessageSent += OnMessageSentCallback;
            List<IPeerNeighbor> neighbors = this.Neighbors;

            foreach (IPeerNeighbor neighbor in neighbors)
            {
                if (neighbor.Equals(except))
                    continue;
                // Don't do anything if the neighbor is not connected
                if (PeerNeighborStateHelper.IsConnected(neighbor.State))
                {
                    Message fmessage = messageBuffer.CreateMessage();
                    if (index != -1)
                    {
                        fmessage.Headers.ReplaceAt(index, hopHeader);
                    }

                    // Don't do anything if the neighbor is not connected
                    if (PeerNeighborStateHelper.IsConnected(neighbor.State))
                    {
                        BeginSendHelper(neighbor, timeout, fmessage, fresult);
                    }
                }
            }
            fresult.MarkEnd(true);
            return fresult;

        }

        public void Open()
        {
            OnOpen();
        }

        public void Close()
        {
            OnClose();
        }

        public abstract void OnOpen();

        public abstract void OnClose();

        public virtual void OnNeighborConnected(IPeerNeighbor neighbor)
        {
            this.neighbors = this.neighborManager.GetConnectedNeighbors();
        }

        public virtual void OnNeighborClosed(IPeerNeighbor neighbor)
        {
            this.neighbors = this.neighborManager.GetConnectedNeighbors();
        }

        public abstract void ProcessLinkUtility(IPeerNeighbor neighbor, TLinkContract utilityInfo);

        public abstract bool ShouldProcess(TFloodContract floodInfo);
        public abstract void RecordOutgoingMessage(byte[] id);

        int UpdateHopCount(Message message, out MessageHeader hopHeader, out ulong currentValue)
        {
            int index = -1;
            currentValue = PeerTransportConstants.MaxHopCount;
            hopHeader = null;
            try
            {
                // If a message contains multiple Hopcounts with our name and namespace or the message can't deserialize to a ulong then ignore the HopCount
                index = message.Headers.FindHeader(PeerStrings.HopCountElementName, PeerStrings.HopCountElementNamespace);
                if (index != -1)
                {
                    currentValue = PeerMessageHelpers.GetHeaderULong(message.Headers, index);
                    hopHeader = MessageHeader.CreateHeader(PeerStrings.HopCountElementName, PeerStrings.HopCountElementNamespace, --currentValue, false);
                }
            }
            catch (MessageHeaderException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);
            }
            catch (CommunicationException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);
            }
            catch (SerializationException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);
            }
            catch (XmlException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);
            }
            Fx.Assert((index == -1) || (hopHeader != null), "Could not successfully create new HopCount Header!");
            return index;
        }

        public virtual IAsyncResult OnFloodedMessage(IPeerNeighbor neighbor, TFloodContract floodInfo, AsyncCallback callback, object state)
        {
            bool process = false;
            MessageBuffer messageBuffer = null;
            Message message = null;
            Uri via;
            Uri to;
            int index = 0;
            ulong remainingHops = PeerTransportConstants.MaxHopCount;
            MessageHeader hopHeader = null;
            bool fatal = false;
            PeerMessageProperty peerProperty = null;
            IAsyncResult result = null;

            try
            {
                peerProperty = (PeerMessageProperty)floodInfo.Properties[PeerStrings.PeerProperty];
                if (!peerProperty.MessageVerified)
                {
                    if (peerProperty.CacheMiss > UtilityExtension.AcceptableMissDistance)
                    {
                        UtilityExtension.ReportCacheMiss(neighbor, peerProperty.CacheMiss);
                    }
                    result = new CompletedAsyncResult(callback, state);
                }
                else
                {
                    process = true;
                    messageBuffer = floodInfo.CreateBufferedCopy((int)this.config.MaxReceivedMessageSize);
                    message = messageBuffer.CreateMessage();
                    via = peerProperty.PeerVia;
                    to = peerProperty.PeerTo;
                    message.Headers.To = message.Properties.Via = via;

                    index = UpdateHopCount(message, out hopHeader, out remainingHops);

                    PeerMessagePropagation propagateFlags = PeerMessagePropagation.LocalAndRemote;
                    if (peerProperty.SkipLocalChannels)
                        propagateFlags = PeerMessagePropagation.Remote;
                    else if (messageHandler.HasMessagePropagation)
                    {
                        using (Message filterMessage = messageBuffer.CreateMessage())
                        {
                            propagateFlags = messageHandler.DetermineMessagePropagation(filterMessage, PeerMessageOrigination.Remote);
                        }
                    }

                    if ((propagateFlags & PeerMessagePropagation.Remote) != 0)
                    {
                        if (remainingHops == 0)
                            propagateFlags &= ~PeerMessagePropagation.Remote;
                    }
                    if ((propagateFlags & PeerMessagePropagation.Remote) != 0)
                    {
                        result = BeginFloodReceivedMessage(neighbor, messageBuffer, PeerTransportConstants.ForwardTimeout, callback, state, index, hopHeader);
                    }
                    else
                    {
                        result = new CompletedAsyncResult(callback, state);
                    }
                    if ((propagateFlags & PeerMessagePropagation.Local) != 0)
                    {
                        messageHandler.HandleIncomingMessage(messageBuffer, propagateFlags, index, hopHeader, via, to);
                    }
                }
                UtilityExtension.UpdateLinkUtility(neighbor, process);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    fatal = true;
                    throw;
                }
                if (null != CloseNeighborIfKnownException(neighborManager, e, neighbor))
                {
                    throw;
                }
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            finally
            {
                if (!fatal)
                {
                    if (message != null)
                        message.Close();
                    if (messageBuffer != null)
                        messageBuffer.Close();
                }
            }
            return result;
        }

        public virtual void EndFloodMessage(IAsyncResult result)
        {
            if (result is CompletedAsyncResult)
            {
                CompletedAsyncResult.End(result);
                return;
            }
            FloodAsyncResult fresult = result as FloodAsyncResult;
            Fx.Assert(fresult != null, "Invalid AsyncResult type in EndFloodResult");
            fresult.End();

        }

        protected long MaxReceivedMessageSize
        {
            get { return config.MaxReceivedMessageSize; }
        }

        protected MessageEncoder MessageEncoder
        {
            get { return config.MessageEncoder; }
        }

        protected object ThisLock
        {
            get { return this.thisLock; }
        }

        protected List<IPeerNeighbor> Neighbors
        {
            get { return this.neighbors; }
        }

        // Guaranteed not to throw anything other than fatal exceptions
        static internal Exception CloseNeighborIfKnownException(PeerNeighborManager neighborManager, Exception exception, IPeerNeighbor peer)
        {
            try
            {
                //ignore this one since the channel is already closed.
                if (exception is ObjectDisposedException)
                    return null;
                else if (
                    (exception is CommunicationException && !(exception.InnerException is QuotaExceededException))
                    || (exception is TimeoutException)
                    || (exception is InvalidOperationException)
                    || (exception is MessageSecurityException)
                )
                {
                    //is this the right close reason?
                    neighborManager.CloseNeighbor(peer, PeerCloseReason.InternalFailure, PeerCloseInitiator.LocalNode, exception);
                    return null;
                }
                else
                {
                    //exception that we dont know or cant act on. 
                    //we will throw this exception to the user.
                    return exception;
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e)) throw;
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                return e;
            }
        }

        public static void EndFloodEncodedMessage(IAsyncResult result)
        {
            CompletedAsyncResult cresult = result as CompletedAsyncResult;
            if (cresult != null)
                CompletedAsyncResult.End(result);
            else
            {
                FloodAsyncResult fresult = result as FloodAsyncResult;
                if (fresult == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("result", SR.GetString(SR.InvalidAsyncResult));
                fresult.End();
            }
        }

        public void EndFloodReceivedMessage(IAsyncResult result)
        {
            FloodAsyncResult fresult = result as FloodAsyncResult;
            Fx.Assert(fresult != null, "Invalid FloodAsyncResult instance during EndFloodReceivedMessage");
        }


        public class PeerThrottleHelper
        {
            int outgoingEnqueuedCount = 0;
            int outgoingQuota = 128;
            IFlooderForThrottle flooder;


            public PeerThrottleHelper(IFlooderForThrottle flooder, int outgoingLimit)
            {
                this.outgoingQuota = outgoingLimit;
                this.flooder = flooder;
            }

            public void ItemDequeued()
            {
                Interlocked.Decrement(ref outgoingEnqueuedCount);
            }

            public void AcquireNoQueue()
            {
                int value = Interlocked.Increment(ref outgoingEnqueuedCount);
                if (value >= outgoingQuota)
                {
                    flooder.OnThrottleReached();
                }
            }
        }
    }

    class PeerFlooderSimple : PeerFlooderBase<Message, UtilityInfo>
    {
        ListManager messageIds;
        const uint MaxBuckets = 5;

        internal PeerFlooderSimple(PeerNodeConfig config, PeerNeighborManager neighborManager)
            : base(config, neighborManager)
        {
            //we want a message id cache that holds message ids for atmost 5 mins.
            this.messageIds = new ListManager(MaxBuckets);
        }

        public override bool ShouldProcess(Message message)
        {
            return message.Properties.ContainsKey(PeerStrings.MessageVerified);
        }
        public bool IsNotSeenBefore(Message message, out byte[] id, out int cacheHit)
        {
            cacheHit = -1;
            id = PeerNodeImplementation.DefaultId;
            if (message is SecurityVerifiedMessage)
            {
                id = (message as SecurityVerifiedMessage).PrimarySignatureValue;

            }
            else
            {
                System.Xml.UniqueId messageId = PeerMessageHelpers.GetHeaderUniqueId(message.Headers, PeerStrings.MessageId, PeerStrings.Namespace);
                if (messageId == null)
                    return false;
                if (messageId.IsGuid)
                {
                    id = new byte[16];
                    messageId.TryGetGuid(id, 0);
                }
                else
                    return false;
            }
            cacheHit = messageIds.AddForLookup(id);
            if (cacheHit == -1)
            {
                return true;
            }
            return false;

        }

        public override void RecordOutgoingMessage(byte[] id)
        {
            this.messageIds.AddForFlood(id);
        }

        public override void OnOpen()
        {
        }

        public override void OnClose()
        {
            this.messageIds.Close();
        }


        public override IAsyncResult OnFloodedMessage(IPeerNeighbor neighbor, Message floodInfo, AsyncCallback callback, object state)
        {
            return base.OnFloodedMessage(neighbor, floodInfo, callback, state);
        }

        public override void EndFloodMessage(IAsyncResult result)
        {
            base.EndFloodMessage(result);

        }

        public override void ProcessLinkUtility(IPeerNeighbor neighbor, UtilityInfo utilityInfo)
        {
            if (!PeerNeighborStateHelper.IsConnected(neighbor.State))
            {
                neighbor.Abort(PeerCloseReason.InvalidNeighbor, PeerCloseInitiator.LocalNode);
                return;
            }

            try
            {
                UtilityExtension.ProcessLinkUtility(neighbor, utilityInfo);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e)) throw;
                if (null != CloseNeighborIfKnownException(neighborManager, e, neighbor))
                {
                    throw;
                }
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
        }

        class ListManager
        {
            uint active;            //current bucket.
            readonly uint buckets;
            
            // Double-checked locking pattern requires volatile for read/write synchronization
            volatile bool disposed = false;
            IOThreadTimer messagePruningTimer;
            //we service the hashtables every one minute
            static readonly int PruningTimout = 60 * 1000;
            static readonly int InitialCount = 1000;
            Dictionary<byte[], bool>[] tables;
            //Hashtable[] tables;
            object thisLock;
            static InMemoryNonceCache.NonceCacheImpl.NonceKeyComparer keyComparer = new InMemoryNonceCache.NonceCacheImpl.NonceKeyComparer();
            const int NotFound = -1;
            //creating this ListManager with n implies that the entries will be available for n minutes atmost.
            //in the n+1 minute, the timer message handler will kick in to clear older messages.
            //every minute, the 
            public ListManager(uint buckets)
            {
                if (!(buckets > 1))
                {
                    throw Fx.AssertAndThrow("ListManager should be used atleast with 2 buckets");
                }
                this.buckets = buckets;
                tables = new Dictionary<byte[], bool>[buckets];

                for (uint i = 0; i < buckets; i++)
                {
                    tables[i] = NewCache(InitialCount);
                }
                //create a timer and kickit off for 1 minute
                messagePruningTimer = new IOThreadTimer(new Action<object>(OnTimeout), null, false);
                messagePruningTimer.Set(PruningTimout);
                this.active = 0;
                this.disposed = false;
                this.thisLock = new object();
            }

            object ThisLock
            {
                get
                {
                    return thisLock;
                }
            }

            public int AddForLookup(byte[] key)
            {
                int table = NotFound;
                if (disposed)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.PeerFlooderDisposed)));
                }

                lock (ThisLock)
                {
                    if ((table = Contains(key)) == NotFound)
                    {
                        tables[active].Add(key, false);
                    }
                    return table;
                }
            }

            public bool AddForFlood(byte[] key)
            {
                if (disposed)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.PeerFlooderDisposed)));
                }

                lock (ThisLock)
                {
                    if (UpdateFloodEntry(key))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            internal void Close()
            {
                lock (ThisLock)
                {
                    if (disposed)
                        return;
                    messagePruningTimer.Cancel();
                    messagePruningTimer = null;
                    tables = null;
                    disposed = true;
                }
            }

            //it does not use locks and expects the caller to hold the lock.
            internal bool UpdateFloodEntry(byte[] key)
            {
                bool flooded = false;
                //check if the message is present in any of the buckets.
                //assumption is that a hit is likely in the current or most recent bucket.
                //we start looking in the current active table and then in the previous and then backwards ...
                for (uint i = buckets; i > 0; i--)
                {
                    if (tables[(active + i) % buckets].TryGetValue(key, out flooded))
                    {
                        if (!flooded)
                        {
                            tables[(active + i) % buckets][key] = true;
                            return true;
                        }
                        else
                            return false;
                    }
                }
                tables[active].Add(key, true);
                return true;
            }

            //it does not use locks and expects the caller to hold the lock.
            internal int Contains(byte[] key)
            {
                int cache = NotFound;
                uint i = 0;
                //check if the message is present in any of the buckets.
                //assumption is that a hit is likely in the current or most recent bucket.
                //we start looking in the current active table and then in the previous and then backwards ...
                for (i = buckets; i > 0; i--)
                {
                    if (tables[(active + i) % buckets].ContainsKey(key))
                        cache = (int)i;
                }
                if (cache < 0)
                    return cache;
                cache = (int)((active + buckets - i) % buckets);
                return cache;
            }

            void OnTimeout(object state)
            {
                if (disposed)
                    return;
                lock (ThisLock)
                {
                    if (disposed)
                        return;
                    active = (active + 1) % (buckets);
                    tables[active] = NewCache(tables[active].Count);
                    messagePruningTimer.Set(PruningTimout);
                }
            }

            Dictionary<byte[], bool> NewCache(int capacity)
            {
                return new Dictionary<byte[], bool>(capacity, keyComparer);
            }
        }
    }


    // this class should contain a collection of IAsyncResults returned from neighbor.BeginSend
    // and complete once all sends have completed
    class FloodAsyncResult : AsyncResult
    {
        bool doneAdding = false;
        Exception exception;
        PeerNeighborManager pnm;

        // Double-checked locking pattern requires volatile for read/write synchronization
        volatile bool isCompleted = false;
        //async results who signaled completion but we have not called EndSend.
        List<IAsyncResult> pending = new List<IAsyncResult>();
        Dictionary<IAsyncResult, IPeerNeighbor> results = new Dictionary<IAsyncResult, IPeerNeighbor>();
        bool shouldCallComplete = false;
        object thisLock = new object();
        TimeoutHelper timeoutHelper;
        bool offNode = false;
        public event EventHandler OnMessageSent;


        public FloodAsyncResult(PeerNeighborManager owner, TimeSpan timeout, AsyncCallback callback, object state)
            : base(callback, state)
        {
            this.pnm = owner;
            this.timeoutHelper = new TimeoutHelper(timeout);
        }

        object ThisLock
        {
            get
            {
                return thisLock;
            }
        }

        public void AddResult(IAsyncResult result, IPeerNeighbor neighbor)
        {
            lock (ThisLock)
            {
                this.results.Add(result, neighbor);
            }
        }

        //user wants to end business. This method is called as a result of EndSend on the flooder.
        //internal methods do not call this. we are asserting that this method should not be called in case of failed BeginX
        public void End()
        {
            if (!(this.doneAdding && this.shouldCallComplete))
            {
                throw Fx.AssertAndThrow("Unexpected end!");
            }
            if (this.isCompleted)
            {
                return;
            }

            //simply wait on the base's event handle
            bool completed = TimeoutHelper.WaitOne(this.AsyncWaitHandle, this.timeoutHelper.RemainingTime());
            if (!completed)
            {
                // a time out occurred - if mo message went off node then tell AsyncResult to throw.
                if (!offNode)
                {
                    try
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException());
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e)) throw;
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                        this.exception = e;
                    }
                }
                //otherwise trace that the timeout was not sufficient for complete send
                lock (ThisLock)
                {
                    if (this.isCompleted)
                        return;
                    this.isCompleted = true;
                }
                CompleteOp(false);
            }
            AsyncResult.End<FloodAsyncResult>(this);
        }

        //this method marks the end of BeginX by the flooder.
        //if there were errors during BeginX, this method may be prematurely called
        //in this case, our only job is to call EndX on successful BeginX calls. we do not report back to caller in this case.
        //base.Complete will not be called and End() will not be called. User has already received exception during BeginX
        //if there was no exception during BeginX, excep param is null. In this case, we call base.Complete upon the last EndX
        public void MarkEnd(bool success)
        {
            bool callComplete = false;
            try
            {
                lock (this.ThisLock)
                {
                    foreach (IAsyncResult result in pending)
                    {
                        OnSendComplete(result);
                    }
                    pending.Clear();
                    this.doneAdding = true;
                    this.shouldCallComplete = success; //only call base.Complete if there is no error during BeginX
                    if (this.results.Count == 0)
                    {
                        this.isCompleted = true;
                        callComplete = true;
                    }
                }
            }
            finally
            {
                if (callComplete)
                {
                    CompleteOp(true);
                }
            }

        }


        //this is the callback routine for async completion on channel BeginSend() operations.
        //if we are done, simply return. This can happen if user called [....] EndX.
        //if the flooder is still processing BeginSend(), then we probably cant complete. In this case, add the result to pending and return
        //main thread will flush the pending completions in MarkEnd().
        //otherwise, call EndX on the result and remove it from results.
        //if this is the last invoke, signal user using base.Complete AND isCompleted=true
        internal void OnSendComplete(IAsyncResult result)
        {
            bool callComplete = false;
            IPeerNeighbor neighbor = null;
            bool fatal = false;
            if (isCompleted)
                return;
            Message message = (Message)result.AsyncState;

            //wait until flooder had a chance to call all outgoing channels and give us Async results.
            lock (ThisLock)
            {
                if (isCompleted)
                    return;

                if (!this.results.TryGetValue(result, out neighbor))
                {
                    if (!doneAdding)
                        this.pending.Add(result);
                    else
                    {
                        throw Fx.AssertAndThrow("IAsyncResult is un-accounted for.");
                    }
                    return;
                }
                this.results.Remove(result);

                try
                {
                    //try doing this only if the async result is marked !CompletedSynchronously. 
                    if (!result.CompletedSynchronously)
                    {
                        neighbor.EndSend(result);
                        offNode = true;
                        UtilityExtension.OnEndSend(neighbor, this);
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        fatal = true;
                        throw;
                    }

                    Exception temp = PeerFlooder.CloseNeighborIfKnownException(pnm, e, neighbor);
                    //we want to return the very first exception to the user. 
                    if (temp != null && this.doneAdding && !this.shouldCallComplete)
                        throw;
                    if (this.exception == null)
                    {
                        this.exception = temp;
                    }
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
                finally
                {
                    if (message != null && !result.CompletedSynchronously && !fatal)
                        message.Close();
                }
                //dont want to call Complete from the lock. 
                //we just decide if this thread should call complete and call outside the lock.
                if (this.results.Count == 0 && this.doneAdding && this.shouldCallComplete)
                {
                    this.isCompleted = true;
                    callComplete = true;
                }
            }
            //if we are done with callbacks and beginx calls, 
            if (callComplete && this.shouldCallComplete)
            {
                CompleteOp(false);
            }
        }

        void CompleteOp(bool sync)
        {
            //call the callback upon finish
            OnMessageSent(this, EventArgs.Empty);
            base.Complete(sync, this.exception);
        }

    }
}
