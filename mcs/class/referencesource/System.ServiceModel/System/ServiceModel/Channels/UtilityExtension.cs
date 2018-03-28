//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Threading;

    class UtilityExtension : IExtension<IPeerNeighbor>
    {
        uint linkUtility;
        uint updateCount;
        IOThreadTimer ackTimer;
        const uint linkUtilityIncrement = 128;
        const uint maxLinkUtility = 4096;
        int outTotal;
        uint inTotal;
        uint inUseful;
        IPeerNeighbor owner;
        object thisLock = new object();
        object throttleLock = new object();
        public event EventHandler UtilityInfoReceived;
        public event EventHandler UtilityInfoSent;
        TypedMessageConverter messageConverter;
        public const int AcceptableMissDistance = 2;
        int pendingSends = 0;
        int checkPointPendingSends = 0;
        bool isMonitoring = false;
        int expectedClearance;
        IOThreadTimer pruneTimer;
        const int PruneIntervalMilliseconds = 10000;
        TimeSpan pruneInterval;
        const int MinimumPendingMessages = 8;
        public delegate void PruneNeighborCallback(IPeerNeighbor peer);
        PruneNeighborCallback pruneNeighbor;

        UtilityExtension()
        {
            ackTimer = new IOThreadTimer(new Action<object>(AcknowledgeLoop), null, false);
            pendingSends = 0;
            pruneTimer = new IOThreadTimer(new Action<object>(VerifyCheckPoint), null, false);
            pruneInterval = TimeSpan.FromMilliseconds(PruneIntervalMilliseconds + new Random(Process.GetCurrentProcess().Id).Next(PruneIntervalMilliseconds));
        }

        public bool IsAccurate
        {
            get { return updateCount >= 32; }
        }

        public uint LinkUtility
        {
            get
            {
                return linkUtility;
            }
        }

        internal TypedMessageConverter MessageConverter
        {
            get
            {
                if (messageConverter == null)
                {
                    messageConverter = TypedMessageConverter.Create(typeof(UtilityInfo), PeerStrings.LinkUtilityAction);
                }
                return messageConverter;
            }
        }

        public void Attach(IPeerNeighbor host)
        {
            this.owner = host;
            ackTimer.Set(PeerTransportConstants.AckTimeout);
        }

        static public void OnNeighborConnected(IPeerNeighbor neighbor)
        {
            Fx.Assert(neighbor != null, "Neighbor must have a value");
            neighbor.Extensions.Add(new UtilityExtension());
        }

        static public void OnNeighborClosed(IPeerNeighbor neighbor)
        {
            Fx.Assert(neighbor != null, "Neighbor must have a value");
            UtilityExtension ext = neighbor.Extensions.Find<UtilityExtension>();
            if (ext != null) neighbor.Extensions.Remove(ext);
        }

        public void Detach(IPeerNeighbor host)
        {
            ackTimer.Cancel();
            owner = null;

            lock (throttleLock)
            {
                pruneTimer.Cancel();
            }
        }

        public object ThisLock
        {
            get
            {
                return thisLock;
            }
        }

        public static void OnMessageSent(IPeerNeighbor neighbor)
        {
            UtilityExtension ext = neighbor.Extensions.Find<UtilityExtension>();
            if (ext != null) ext.OnMessageSent();
        }

        void OnMessageSent()
        {
            lock (ThisLock)
            {
                outTotal++;
            }
            Interlocked.Increment(ref pendingSends);
        }

        public static void OnEndSend(IPeerNeighbor neighbor, FloodAsyncResult fresult)
        {
            if (neighbor.State >= PeerNeighborState.Disconnecting)
                return;
            UtilityExtension instance = neighbor.Utility;
            if (instance == null)
                return;
            instance.OnEndSend(fresult);
        }

        public void OnEndSend(FloodAsyncResult fresult)
        {
            Interlocked.Decrement(ref pendingSends);
        }

        void AcknowledgeLoop(object state)
        {
            IPeerNeighbor peer = owner;
            if (peer == null || !peer.IsConnected)
                return;
            FlushAcknowledge();
            if (owner != null)
                ackTimer.Set(PeerTransportConstants.AckTimeout);
        }

        static public void ProcessLinkUtility(IPeerNeighbor neighbor, UtilityInfo umessage)
        {
            Fx.Assert(neighbor != null, "Neighbor must have a value");
            UtilityExtension ext = neighbor.Extensions.Find<UtilityExtension>();
            if (ext != null)
            {
                ext.ProcessLinkUtility(umessage.Useful, umessage.Total);
            }
        }

        // Update link utility for the neighbor. received from the neighbor
        void ProcessLinkUtility(uint useful, uint total)
        {
            uint i = 0;
            lock (ThisLock)
            {
                if (total > PeerTransportConstants.AckWindow
                    || useful > total
                    || (uint)outTotal < total
                    )
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.PeerLinkUtilityInvalidValues, useful, total)));
                }

                //VERIFY with in this range, we are hoping that the order of useful/useless messages doesnt matter much. 
                for (i = 0; i < useful; i++)
                {
                    this.linkUtility = Calculate(this.linkUtility, true);
                }
                for (; i < total; i++)
                {
                    this.linkUtility = Calculate(this.linkUtility, false);
                }
                outTotal -= (int)total;
            }
            if (UtilityInfoReceived != null)
            {
                UtilityInfoReceived(this, EventArgs.Empty);
            }

        }

        uint Calculate(uint current, bool increase)
        {
            uint utility = 0;
            // Refer to graph maintenance white paper for explanation of the formula
            // used to compute utility index.
            utility = (uint)current * 31 / 32;
            if (increase)
                utility += linkUtilityIncrement;
            if (!(utility <= maxLinkUtility))
            {
                throw Fx.AssertAndThrow("Link utility should not exceed " + maxLinkUtility);
            }
            if (!IsAccurate)
                ++updateCount;
            return utility;
        }

        public static uint UpdateLinkUtility(IPeerNeighbor neighbor, bool useful)
        {
            Fx.Assert(neighbor != null, "Neighbor must have a value");
            uint linkUtility = 0;
            UtilityExtension ext = neighbor.Extensions.Find<UtilityExtension>();
            if (ext != null)
            {
                // Can happen if the neighbor has been closed for instance
                linkUtility = ext.UpdateLinkUtility(useful);
            }
            return linkUtility;
        }

        public uint UpdateLinkUtility(bool useful)
        {
            lock (ThisLock)
            {
                inTotal++;
                if (useful)
                    inUseful++;
                linkUtility = Calculate(linkUtility, useful);
                if (inTotal == PeerTransportConstants.AckWindow)
                {
                    FlushAcknowledge();
                }
            }
            return linkUtility;
        }

        public void FlushAcknowledge()
        {
            if (inTotal == 0)
                return;
            uint tempUseful = 0, tempTotal = 0;
            lock (ThisLock)
            {
                tempUseful = inUseful;
                tempTotal = inTotal;
                inUseful = 0;
                inTotal = 0;
            }
            SendUtilityMessage(tempUseful, tempTotal);
        }

        class AsyncUtilityState
        {
            public Message message;
            public UtilityInfo info;
            public AsyncUtilityState(Message message, UtilityInfo info)
            {
                this.message = message;
                this.info = info;
            }
        }

        void SendUtilityMessage(uint useful, uint total)
        {
            IPeerNeighbor host = owner;
            if (host == null || !PeerNeighborStateHelper.IsConnected(host.State) || total == 0)
                return;
            UtilityInfo umessage = new UtilityInfo(useful, total);
            IAsyncResult result = null;
            Message message = MessageConverter.ToMessage(umessage, MessageVersion.Soap12WSAddressing10);
            bool fatal = false;
            try
            {
                result = host.BeginSend(message, Fx.ThunkCallback(new AsyncCallback(UtilityMessageSent)), new AsyncUtilityState(message, umessage));
                if (result.CompletedSynchronously)
                {
                    host.EndSend(result);
                    EventHandler handler = UtilityInfoSent;
                    if (handler != null)
                        handler(this, EventArgs.Empty);
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    fatal = true;
                    throw;
                }
                if (null != HandleSendException(host, e, umessage))
                    throw;
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            finally
            {
                if (!fatal && (result == null || result.CompletedSynchronously))
                    message.Close();
            }
        }

        void UtilityMessageSent(IAsyncResult result)
        {
            if (result == null || result.AsyncState == null)
                return;
            IPeerNeighbor host = this.owner;
            if (host == null || !PeerNeighborStateHelper.IsConnected(host.State))
                return;
            if (result.CompletedSynchronously)
                return;

            AsyncUtilityState state = (AsyncUtilityState)result.AsyncState;
            Fx.Assert(state != null, "IAsyncResult.AsyncState does not contain AsyncUtilityState");
            Message message = state.message;
            UtilityInfo umessage = state.info;
            bool fatal = false;
            if (!(umessage != null))
            {
                throw Fx.AssertAndThrow("expecting a UtilityInfo message in the AsyncState!");
            }

            try
            {
                host.EndSend(result);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    fatal = true;
                    throw;
                }
                if (null != HandleSendException(host, e, umessage))
                    throw;
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            finally
            {
                if (!fatal)
                {
                    Fx.Assert(!result.CompletedSynchronously, "result.CompletedSynchronously");
                    message.Close();
                }
            }
            EventHandler handler = UtilityInfoSent;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        Exception HandleSendException(IPeerNeighbor host, Exception e, UtilityInfo umessage)
        {
            if ((e is ObjectDisposedException) ||
                (e is TimeoutException) ||
                (e is CommunicationException))
            {
                if (!(!(e.InnerException is QuotaExceededException)))
                {
                    throw Fx.AssertAndThrow("insufficient quota for sending messages!");
                }
                lock (ThisLock)
                {
                    this.inTotal += umessage.Total;
                    this.inUseful += umessage.Useful;
                }
                return null;
            }
            else
            {
                return e;
            }

        }

        static internal void ReportCacheMiss(IPeerNeighbor neighbor, int missedBy)
        {
            Fx.Assert(missedBy > AcceptableMissDistance, "Call this method for cache misses ONLY!");
            Fx.Assert(neighbor != null, "Neighbor must have a value");

            if (!neighbor.IsConnected)
                return;
            UtilityExtension ext = neighbor.Extensions.Find<UtilityExtension>();
            if (ext != null)
            {
                ext.ReportCacheMiss(missedBy);
            }
        }

        void ReportCacheMiss(int missedBy)
        {
            lock (ThisLock)
            {
                for (int i = 0; i < missedBy; i++)
                {
                    this.linkUtility = Calculate(this.linkUtility, false);
                }
            }
        }

        public int PendingMessages
        {
            get
            {
                return this.pendingSends;
            }
        }

        public void BeginCheckPoint(PruneNeighborCallback pruneCallback)
        {
            if (this.isMonitoring)
                return;

            lock (throttleLock)
            {
                if (this.isMonitoring)
                    return;
                this.checkPointPendingSends = this.pendingSends;
                this.pruneNeighbor = pruneCallback;
                this.expectedClearance = this.pendingSends / 2;
                this.isMonitoring = true;
                if (owner == null)
                    return;
                pruneTimer.Set(pruneInterval);
            }

        }

        void VerifyCheckPoint(object state)
        {
            int lclPendingSends;
            int lclCheckPointPendingSends;
            IPeerNeighbor peer = (IPeerNeighbor)owner;

            if (peer == null || !peer.IsConnected)
                return;

            lock (throttleLock)
            {
                lclPendingSends = this.pendingSends;
                lclCheckPointPendingSends = this.checkPointPendingSends;
            }
            if (lclPendingSends <= MinimumPendingMessages)
            {
                lock (throttleLock)
                {
                    isMonitoring = false;
                }
            }
            else if (lclPendingSends + this.expectedClearance >= lclCheckPointPendingSends)
            {
                pruneNeighbor(peer);
            }
            else
            {
                lock (throttleLock)
                {
                    if (owner == null)
                        return;
                    this.checkPointPendingSends = this.pendingSends;
                    this.expectedClearance = this.expectedClearance / 2;
                    pruneTimer.Set(pruneInterval);
                }

            }
        }
    }
}
