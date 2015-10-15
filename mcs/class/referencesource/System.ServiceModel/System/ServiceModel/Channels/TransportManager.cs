//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Threading;
    using System.ServiceModel.Diagnostics.Application;

    abstract class TransportManager
    {
        ServiceModelActivity activity;
        int openCount;
        object thisLock = new object();

        protected ServiceModelActivity Activity
        {
            get { return this.activity; }
        }

        internal abstract string Scheme { get; }

        internal object ThisLock
        {
            get { return this.thisLock; }
        }

        internal void Close(TransportChannelListener channelListener, TimeSpan timeout)
        {
            this.Cleanup(channelListener, timeout, false);
        }

        void Cleanup(TransportChannelListener channelListener, TimeSpan timeout, bool aborting)
        {
            using (ServiceModelActivity.BoundOperation(this.Activity))
            {
                this.Unregister(channelListener);
            }
            lock (ThisLock)
            {
                if (openCount <= 0)
                {
                    throw Fx.AssertAndThrow("Invalid Open/Close state machine.");
                }

                openCount--;

                if (openCount == 0)
                {
                    // Wrap the final close here with transfers.
                    using (ServiceModelActivity.BoundOperation(this.Activity, true))
                    {
                        if (aborting)
                        {
                            OnAbort();
                        }
                        else
                        {
                            OnClose(timeout);
                        }
                    }
                    if (this.Activity != null)
                    {
                        this.Activity.Dispose();
                    }
                }
            }
        }

        internal static void EnsureRegistered<TChannelListener>(UriPrefixTable<TChannelListener> addressTable,
            TChannelListener channelListener, HostNameComparisonMode registeredComparisonMode)
            where TChannelListener : TransportChannelListener
        {
            TChannelListener existingFactory;
            if (!addressTable.TryLookupUri(channelListener.Uri, registeredComparisonMode, out existingFactory) ||
                (existingFactory != channelListener))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(
                    SR.ListenerFactoryNotRegistered, channelListener.Uri)));
            }
        }

        // Must be called under lock(ThisLock).
        protected void Fault<TChannelListener>(UriPrefixTable<TChannelListener> addressTable, Exception exception)
            where TChannelListener : ChannelListenerBase
        {
            foreach (KeyValuePair<BaseUriWithWildcard, TChannelListener> pair in addressTable.GetAll())
            {
                TChannelListener listener = pair.Value;
                listener.Fault(exception);
                listener.Abort();
            }
        }

        internal abstract void OnClose(TimeSpan timeout);
        internal abstract void OnOpen();
        internal virtual void OnAbort() { }

        internal void Open(TransportChannelListener channelListener)
        {
            if (DiagnosticUtility.ShouldUseActivity)
            {
                if (this.activity == null)
                {
                    this.activity = ServiceModelActivity.CreateActivity(true);
                    if (DiagnosticUtility.ShouldUseActivity)
                    {
                        if (null != FxTrace.Trace)
                        {
                            FxTrace.Trace.TraceTransfer(this.Activity.Id);
                        }
                        ServiceModelActivity.Start(this.Activity, SR.GetString(SR.ActivityListenAt, channelListener.Uri.ToString()), ActivityType.ListenAt);
                    }
                }
                channelListener.Activity = this.Activity;
            }
            using (ServiceModelActivity.BoundOperation(this.Activity))
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.TransportListen,
                        SR.GetString(SR.TraceCodeTransportListen, channelListener.Uri.ToString()), this);
                }
                this.Register(channelListener);
                try
                {
                    lock (ThisLock)
                    {
                        if (openCount == 0)
                        {
                            OnOpen();
                        }

                        openCount++;
                    }
                }
                catch
                {
                    this.Unregister(channelListener);
                    throw;
                }
            }
        }

        internal void Abort(TransportChannelListener channelListener)
        {
            this.Cleanup(channelListener, TimeSpan.Zero, true);
        }

        internal abstract void Register(TransportChannelListener channelListener);

        // should only call this under ThisLock (unless accessing purely for inspection)
        protected void ThrowIfOpen()
        {
            if (openCount > 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR.GetString(SR.TransportManagerOpen)));
            }
        }

        internal abstract void Unregister(TransportChannelListener channelListener);
    }


    delegate IList<TransportManager> SelectTransportManagersCallback();
    class TransportManagerContainer
    {
        IList<TransportManager> transportManagers;
        TransportChannelListener listener;
        bool closed;
        object tableLock;

        public TransportManagerContainer(TransportChannelListener listener)
        {
            this.listener = listener;
            this.tableLock = listener.TransportManagerTable;
            this.transportManagers = new List<TransportManager>();
        }

        TransportManagerContainer(TransportManagerContainer source)
        {
            this.listener = source.listener;
            this.tableLock = source.tableLock;
            this.transportManagers = new List<TransportManager>();
            for (int i = 0; i < source.transportManagers.Count; i++)
            {
                this.transportManagers.Add(source.transportManagers[i]);
            }
        }

        // copy contents into a new container (used for listener/channel lifetime decoupling)
        public static TransportManagerContainer TransferTransportManagers(TransportManagerContainer source)
        {
            TransportManagerContainer result = null;

            lock (source.tableLock)
            {
                if (source.transportManagers.Count > 0)
                {
                    result = new TransportManagerContainer(source);
                    source.transportManagers.Clear();
                }
            }

            return result;
        }

        public void Abort()
        {
            Close(true, TimeSpan.Zero);
        }

        public IAsyncResult BeginOpen(SelectTransportManagersCallback selectTransportManagerCallback,
            AsyncCallback callback, object state)
        {
            return new OpenAsyncResult(selectTransportManagerCallback, this, callback, state);
        }

        public void EndOpen(IAsyncResult result)
        {
            OpenAsyncResult.End(result);
        }

        public void Open(SelectTransportManagersCallback selectTransportManagerCallback)
        {
            lock (this.tableLock)
            {
                if (closed) // if we've been aborted then don't get transport managers
                {
                    return;
                }

                IList<TransportManager> foundTransportManagers = selectTransportManagerCallback();
                if (foundTransportManagers == null) // nothing to do
                {
                    return;
                }

                for (int i = 0; i < foundTransportManagers.Count; i++)
                {
                    TransportManager transportManager = foundTransportManagers[i];
                    transportManager.Open(this.listener);
                    this.transportManagers.Add(transportManager);
                }
            }
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseAsyncResult(this, callback, timeout, state);
        }

        public void EndClose(IAsyncResult result)
        {
            CloseAsyncResult.End(result);
        }

        public void Close(TimeSpan timeout)
        {
            Close(false, timeout);
        }

        public void Close(bool aborting, TimeSpan timeout)
        {
            if (this.closed)
            {
                return;
            }

            IList<TransportManager> transportManagersCopy;
            lock (this.tableLock)
            {
                if (this.closed)
                {
                    return;
                }

                this.closed = true;

                transportManagersCopy = new List<TransportManager>(this.transportManagers);
                this.transportManagers.Clear();

                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                TimeoutException timeoutException = null;
                foreach (TransportManager transportManager in transportManagersCopy)
                {
                    try
                    {
                        if (!aborting && timeoutException == null)
                        {
                            transportManager.Close(listener, timeoutHelper.RemainingTime());
                        }
                        else
                        {
                            transportManager.Abort(listener);
                        }
                    }
                    catch (TimeoutException ex)
                    {
                        timeoutException = ex;
                        transportManager.Abort(listener);
                    }
                }

                if (timeoutException != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(SR.GetString(SR.TimeoutOnClose, timeout), timeoutException));
                }
            }
        }

        abstract class OpenOrCloseAsyncResult : TraceAsyncResult
        {
            TransportManagerContainer parent;
            static Action<object> scheduledCallback = new Action<object>(OnScheduled);

            protected OpenOrCloseAsyncResult(TransportManagerContainer parent, AsyncCallback callback,
                object state)
                : base(callback, state)
            {
                this.parent = parent;
            }

            protected void Begin()
            {
                ActionItem.Schedule(scheduledCallback, this);
            }

            static void OnScheduled(object state)
            {
                ((OpenOrCloseAsyncResult)state).OnScheduled();
            }

            void OnScheduled()
            {
                using (ServiceModelActivity.BoundOperation(this.CallbackActivity))
                {
                    Exception completionException = null;
                    try
                    {
                        this.OnScheduled(this.parent);
                    }
#pragma warning suppress 56500 // Microsoft, transferring exception to another thread
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        completionException = e;
                    }

                    this.Complete(false, completionException);
                }
            }

            protected abstract void OnScheduled(TransportManagerContainer parent);
        }

        sealed class CloseAsyncResult : OpenOrCloseAsyncResult
        {
            TimeoutHelper timeoutHelper;

            public CloseAsyncResult(TransportManagerContainer parent, AsyncCallback callback, TimeSpan timeout,
                object state)
                : base(parent, callback, state)
            {
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.timeoutHelper.RemainingTime(); //start count down
                this.Begin();
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<CloseAsyncResult>(result);
            }

            protected override void OnScheduled(TransportManagerContainer parent)
            {
                parent.Close(timeoutHelper.RemainingTime());
            }
        }

        sealed class OpenAsyncResult : OpenOrCloseAsyncResult
        {
            SelectTransportManagersCallback selectTransportManagerCallback;

            public OpenAsyncResult(SelectTransportManagersCallback selectTransportManagerCallback, TransportManagerContainer parent,
                AsyncCallback callback, object state)
                : base(parent, callback, state)
            {
                this.selectTransportManagerCallback = selectTransportManagerCallback;
                this.Begin();
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<OpenAsyncResult>(result);
            }

            protected override void OnScheduled(TransportManagerContainer parent)
            {
                parent.Open(this.selectTransportManagerCallback);
            }
        }
    }
}
