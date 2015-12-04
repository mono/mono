//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;

    // code that pools items and closes/aborts them as necessary.
    // shared by IConnection and IChannel users
    abstract class CommunicationPool<TKey, TItem>
        where TKey : class
        where TItem : class
    {
        Dictionary<TKey, EndpointConnectionPool> endpointPools;
        int maxCount;
        int openCount;
        // need to make sure we prune over a certain number of endpoint pools
        int pruneAccrual;
        const int pruneThreshold = 30;

        protected CommunicationPool(int maxCount)
        {
            this.maxCount = maxCount;
            this.endpointPools = new Dictionary<TKey, EndpointConnectionPool>();
            this.openCount = 1;
        }

        public int MaxIdleConnectionPoolCount
        {
            get { return this.maxCount; }
        }

        protected object ThisLock
        {
            get { return this; }
        }

        protected abstract void AbortItem(TItem item);

        [Fx.Tag.Throws(typeof(CommunicationException), "A communication exception occurred closing this item")]
        [Fx.Tag.Throws(typeof(TimeoutException), "Timed out trying to close this item")]
        protected abstract void CloseItem(TItem item, TimeSpan timeout);
        protected abstract void CloseItemAsync(TItem item, TimeSpan timeout);

        protected abstract TKey GetPoolKey(EndpointAddress address, Uri via);

        protected virtual EndpointConnectionPool CreateEndpointConnectionPool(TKey key)
        {
            return new EndpointConnectionPool(this, key);
        }

        public bool Close(TimeSpan timeout)
        {
            lock (ThisLock)
            {
                if (openCount <= 0)
                {
                    return true;
                }

                openCount--;

                if (openCount == 0)
                {
                    this.OnClose(timeout);
                    return true;
                }

                return false;
            }
        }

        List<TItem> PruneIfNecessary()
        {
            List<TItem> itemsToClose = null;
            pruneAccrual++;
            if (pruneAccrual > pruneThreshold)
            {
                pruneAccrual = 0;
                itemsToClose = new List<TItem>();

                // first prune the connection pool contents
                foreach (EndpointConnectionPool pool in endpointPools.Values)
                {
                    pool.Prune(itemsToClose);
                }

                // figure out which connection pools are now empty
                List<TKey> endpointKeysToRemove = null;
                foreach (KeyValuePair<TKey, EndpointConnectionPool> poolEntry in endpointPools)
                {
                    if (poolEntry.Value.CloseIfEmpty())
                    {
                        if (endpointKeysToRemove == null)
                        {
                            endpointKeysToRemove = new List<TKey>();
                        }
                        endpointKeysToRemove.Add(poolEntry.Key);
                    }
                }

                // and then prune the connection pools themselves
                if (endpointKeysToRemove != null)
                {
                    for (int i = 0; i < endpointKeysToRemove.Count; i++)
                    {
                        endpointPools.Remove(endpointKeysToRemove[i]);
                    }
                }
            }

            return itemsToClose;
        }

        EndpointConnectionPool GetEndpointPool(TKey key, TimeSpan timeout)
        {
            EndpointConnectionPool result = null;
            List<TItem> itemsToClose = null;
            lock (ThisLock)
            {
                if (!endpointPools.TryGetValue(key, out result))
                {
                    itemsToClose = PruneIfNecessary();
                    result = CreateEndpointConnectionPool(key);
                    endpointPools.Add(key, result);
                }
            }

            Fx.Assert(result != null, "EndpointPool must be non-null at this point");
            if (itemsToClose != null && itemsToClose.Count > 0)
            {
                // allocate half the remaining timeout for our g----ful shutdowns
                TimeoutHelper timeoutHelper = new TimeoutHelper(TimeoutHelper.Divide(timeout, 2));
                for (int i = 0; i < itemsToClose.Count; i++)
                {
                    result.CloseIdleConnection(itemsToClose[i], timeoutHelper.RemainingTime());
                }
            }

            return result;
        }

        public bool TryOpen()
        {
            lock (ThisLock)
            {
                if (openCount <= 0)
                {
                    // can't reopen connection pools since the registry purges them on close
                    return false;
                }
                else
                {
                    openCount++;
                    return true;
                }
            }
        }

        protected virtual void OnClosed()
        {
        }

        void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            foreach (EndpointConnectionPool pool in endpointPools.Values)
            {
                try
                {
                    pool.Close(timeoutHelper.RemainingTime());
                }
                catch (CommunicationException exception)
                {
                    if (DiagnosticUtility.ShouldTraceError)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Error, TraceCode.ConnectionPoolCloseException, SR.GetString(SR.TraceCodeConnectionPoolCloseException), this, exception);
                    }
                }
                catch (TimeoutException exception)
                {
                    if (TD.CloseTimeoutIsEnabled())
                    {
                        TD.CloseTimeout(exception.Message);
                    }
                    if (DiagnosticUtility.ShouldTraceError)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Error, TraceCode.ConnectionPoolCloseException, SR.GetString(SR.TraceCodeConnectionPoolCloseException), this, exception);
                    }
                }
            }

            endpointPools.Clear();
        }

        public void AddConnection(TKey key, TItem connection, TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            EndpointConnectionPool endpointPool = GetEndpointPool(key, timeoutHelper.RemainingTime());
            endpointPool.AddConnection(connection, timeoutHelper.RemainingTime());
        }

        public TItem TakeConnection(EndpointAddress address, Uri via, TimeSpan timeout, out TKey key)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            key = this.GetPoolKey(address, via);
            EndpointConnectionPool endpointPool = GetEndpointPool(key, timeoutHelper.RemainingTime());
            return endpointPool.TakeConnection(timeoutHelper.RemainingTime());
        }

        public void ReturnConnection(TKey key, TItem connection, bool connectionIsStillGood, TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            EndpointConnectionPool endpointPool = GetEndpointPool(key, timeoutHelper.RemainingTime());
            endpointPool.ReturnConnection(connection, connectionIsStillGood, timeoutHelper.RemainingTime());
        }

        // base class for our collection of Idle connections
        protected abstract class IdleConnectionPool
        {
            public abstract int Count { get; }
            public abstract bool Add(TItem item);
            public abstract bool Return(TItem item);
            public abstract TItem Take(out bool closeItem);
        }

        protected class EndpointConnectionPool
        {
            TKey key;
            List<TItem> busyConnections;
            bool closed;
            IdleConnectionPool idleConnections;
            CommunicationPool<TKey, TItem> parent;

            public EndpointConnectionPool(CommunicationPool<TKey, TItem> parent, TKey key)
            {
                this.key = key;
                this.parent = parent;
                this.busyConnections = new List<TItem>();
            }

            protected TKey Key
            {
                get { return this.key; }
            }

            IdleConnectionPool IdleConnections
            {
                get
                {
                    if (idleConnections == null)
                    {
                        idleConnections = GetIdleConnectionPool();
                    }

                    return idleConnections;
                }
            }

            protected CommunicationPool<TKey, TItem> Parent
            {
                get { return this.parent; }
            }

            protected object ThisLock
            {
                get { return this; }
            }

            // close down the pool if empty
            public bool CloseIfEmpty()
            {
                lock (ThisLock)
                {
                    if (!closed)
                    {
                        if (busyConnections.Count > 0)
                        {
                            return false;
                        }

                        if (idleConnections != null && idleConnections.Count > 0)
                        {
                            return false;
                        }
                        closed = true;
                    }
                }

                return true;
            }

            protected virtual void AbortItem(TItem item)
            {
                parent.AbortItem(item);
            }

            [Fx.Tag.Throws(typeof(CommunicationException), "A communication exception occurred closing this item")]
            [Fx.Tag.Throws(typeof(TimeoutException), "Timed out trying to close this item")]
            protected virtual void CloseItem(TItem item, TimeSpan timeout)
            {
                parent.CloseItem(item, timeout);
            }

            protected virtual void CloseItemAsync(TItem item, TimeSpan timeout)
            {
                parent.CloseItemAsync(item, timeout);
            }

            public void Abort()
            {
                if (closed)
                {
                    return;
                }

                List<TItem> idleItemsToClose = null;
                lock (ThisLock)
                {
                    if (closed)
                        return;

                    closed = true;
                    idleItemsToClose = SnapshotIdleConnections();
                }

                AbortConnections(idleItemsToClose);
            }

            [Fx.Tag.Throws(typeof(CommunicationException), "A communication exception occurred closing this item")]
            [Fx.Tag.Throws(typeof(TimeoutException), "Timed out trying to close this item")]
            public void Close(TimeSpan timeout)
            {
                List<TItem> itemsToClose = null;
                lock (ThisLock)
                {
                    if (closed)
                        return;

                    closed = true;
                    itemsToClose = SnapshotIdleConnections();
                }

                try
                {
                    TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                    for (int i = 0; i < itemsToClose.Count; i++)
                    {
                        this.CloseItem(itemsToClose[i], timeoutHelper.RemainingTime());
                    }

                    itemsToClose.Clear();
                }
                finally
                {
                    AbortConnections(itemsToClose);
                }
            }

            void AbortConnections(List<TItem> idleItemsToClose)
            {
                for (int i = 0; i < idleItemsToClose.Count; i++)
                {
                    this.AbortItem(idleItemsToClose[i]);
                }

                for (int i = 0; i < busyConnections.Count; i++)
                {
                    this.AbortItem(busyConnections[i]);
                }
                busyConnections.Clear();
            }

            // must call under lock (ThisLock) since we are calling IdleConnections.Take()
            List<TItem> SnapshotIdleConnections()
            {
                List<TItem> itemsToClose = new List<TItem>();
                bool dummy;
                for (;;)
                {
                    TItem item = IdleConnections.Take(out dummy);
                    if (item == null)
                        break;

                    itemsToClose.Add(item);
                }

                return itemsToClose;
            }

            public void AddConnection(TItem connection, TimeSpan timeout)
            {
                bool closeConnection = false;
                lock (ThisLock)
                {
                    if (!closed)
                    {
                        if (!IdleConnections.Add(connection))
                        {
                            closeConnection = true;
                        }
                    }
                    else
                    {
                        closeConnection = true;
                    }
                }

                if (closeConnection)
                {
                    CloseIdleConnection(connection, timeout);
                }
            }

            protected virtual IdleConnectionPool GetIdleConnectionPool()
            {
                return new PoolIdleConnectionPool(parent.MaxIdleConnectionPoolCount);
            }

            public virtual void Prune(List<TItem> itemsToClose)
            {
            }

            public TItem TakeConnection(TimeSpan timeout)
            {
                TItem item = null;
                List<TItem> itemsToClose = null;
                lock (ThisLock)
                {
                    if (closed)
                        return null;

                    bool closeItem;
                    while (true)
                    {
                        item = IdleConnections.Take(out closeItem);
                        if (item == null)
                        {
                            break;
                        }

                        if (!closeItem)
                        {
                            busyConnections.Add(item);
                            break;
                        }

                        if (itemsToClose == null)
                        {
                            itemsToClose = new List<TItem>();
                        }
                        itemsToClose.Add(item);
                    }
                }

                // cleanup any stale items accrued from IdleConnections
                if (itemsToClose != null)
                {
                    // and only allocate half the timeout passed in for our g----ful shutdowns
                    TimeoutHelper timeoutHelper = new TimeoutHelper(TimeoutHelper.Divide(timeout, 2));
                    for (int i = 0; i < itemsToClose.Count; i++)
                    {
                        CloseIdleConnection(itemsToClose[i], timeoutHelper.RemainingTime());
                    }
                }

                if (TD.ConnectionPoolMissIsEnabled())
                {
                    if (item == null && busyConnections != null)
                    {
                        TD.ConnectionPoolMiss(key != null ? key.ToString() : string.Empty, busyConnections.Count);
                    }
                }

                return item;
            }

            public void ReturnConnection(TItem connection, bool connectionIsStillGood, TimeSpan timeout)
            {
                bool closeConnection = false;
                bool abortConnection = false;

                lock (ThisLock)
                {
                    if (!closed)
                    {
                        if (busyConnections.Remove(connection) && connectionIsStillGood)
                        {
                            if (!IdleConnections.Return(connection))
                            {
                                closeConnection = true;
                            }
                        }
                        else
                        {
                            abortConnection = true;
                        }
                    }
                    else
                    {
                        abortConnection = true;
                    }
                }

                if (closeConnection)
                {
                    CloseIdleConnection(connection, timeout);
                }
                else if (abortConnection)
                {
                    this.AbortItem(connection);
                    OnConnectionAborted();
                }
            }

            public void CloseIdleConnection(TItem connection, TimeSpan timeout)
            {
                bool throwing = true;
                try
                {
                    this.CloseItemAsync(connection, timeout);
                    throwing = false;
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
                finally
                {
                    if (throwing)
                    {
                        this.AbortItem(connection);
                    }
                }
            }

            protected virtual void OnConnectionAborted()
            {
            }

            protected class PoolIdleConnectionPool
                : IdleConnectionPool
            {
                Pool<TItem> idleConnections;
                int maxCount;

                public PoolIdleConnectionPool(int maxCount)
                {
                    this.idleConnections = new Pool<TItem>(maxCount);
                    this.maxCount = maxCount;
                }

                public override int Count
                {
                    get { return idleConnections.Count; }
                }

                public override bool Add(TItem connection)
                {
                    return ReturnToPool(connection);
                }

                public override bool Return(TItem connection)
                {
                    return ReturnToPool(connection);
                }

                bool ReturnToPool(TItem connection)
                {
                    bool result = this.idleConnections.Return(connection);
                    if (!result)
                    {
                        if (TD.MaxOutboundConnectionsPerEndpointExceededIsEnabled())
                        {
                            TD.MaxOutboundConnectionsPerEndpointExceeded(SR.GetString(SR.TraceCodeConnectionPoolMaxOutboundConnectionsPerEndpointQuotaReached, maxCount));
                        }
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Information,
                                TraceCode.ConnectionPoolMaxOutboundConnectionsPerEndpointQuotaReached,
                                SR.GetString(SR.TraceCodeConnectionPoolMaxOutboundConnectionsPerEndpointQuotaReached, maxCount),
                                this);
                        }
                    }
                    else if (TD.OutboundConnectionsPerEndpointRatioIsEnabled())
                    {
                        TD.OutboundConnectionsPerEndpointRatio(this.idleConnections.Count, maxCount);
                    }

                    return result;
                }

                public override TItem Take(out bool closeItem)
                {
                    closeItem = false;
                    TItem ret = this.idleConnections.Take();
                    if (TD.OutboundConnectionsPerEndpointRatioIsEnabled())
                    {
                        TD.OutboundConnectionsPerEndpointRatio(this.idleConnections.Count, maxCount);
                    }
                    return ret;
                }
            }
        }
    }

    // all our connection pools support Idling out of connections and lease timeout
    // (though Named Pipes doesn't leverage the lease timeout)
    abstract class ConnectionPool : IdlingCommunicationPool<string, IConnection>
    {
        int connectionBufferSize;
        TimeSpan maxOutputDelay;
        string name;

        protected ConnectionPool(IConnectionOrientedTransportChannelFactorySettings settings, TimeSpan leaseTimeout)
            : base(settings.MaxOutboundConnectionsPerEndpoint, settings.IdleTimeout, leaseTimeout)
        {
            this.connectionBufferSize = settings.ConnectionBufferSize;
            this.maxOutputDelay = settings.MaxOutputDelay;
            this.name = settings.ConnectionPoolGroupName;
        }

        public string Name
        {
            get { return this.name; }
        }

        protected override void AbortItem(IConnection item)
        {
            item.Abort();
        }

        protected override void CloseItem(IConnection item, TimeSpan timeout)
        {
            item.Close(timeout, false);
        }

        protected override void CloseItemAsync(IConnection item, TimeSpan timeout)
        {
            item.Close(timeout, true);
        }

        public virtual bool IsCompatible(IConnectionOrientedTransportChannelFactorySettings settings)
        {
            return (
                (this.name == settings.ConnectionPoolGroupName) &&
                (this.connectionBufferSize == settings.ConnectionBufferSize) &&
                (this.MaxIdleConnectionPoolCount == settings.MaxOutboundConnectionsPerEndpoint) &&
                (this.IdleTimeout == settings.IdleTimeout) &&
                (this.maxOutputDelay == settings.MaxOutputDelay)
                );
        }
    }

    // Helper class used to manage the lifetime of a connection relative to its pool.
    abstract class ConnectionPoolHelper
    {
        IConnectionInitiator connectionInitiator;
        ConnectionPool connectionPool;
        Uri via;
        bool closed;

        // key for rawConnection in the connection pool
        string connectionKey;

        // did rawConnection originally come from connectionPool?
        bool isConnectionFromPool;

        // the "raw" connection that should be stored in the pool
        IConnection rawConnection;

        // the "upgraded" connection built on top of the "raw" connection to be used for I/O
        IConnection upgradedConnection;

        EventTraceActivity eventTraceActivity;

        public ConnectionPoolHelper(ConnectionPool connectionPool, IConnectionInitiator connectionInitiator, Uri via)
        {
            this.connectionInitiator = connectionInitiator;
            this.connectionPool = connectionPool;
            this.via = via;
        }

        object ThisLock
        {
            get { return this; }
        }

        protected EventTraceActivity EventTraceActivity
        {
            get
            {
                if (this.eventTraceActivity == null)
                {
                    this.eventTraceActivity = EventTraceActivity.GetFromThreadOrCreate();
                }
                return this.eventTraceActivity;
            }
        }

        protected abstract IConnection AcceptPooledConnection(IConnection connection, ref TimeoutHelper timeoutHelper);
        protected abstract IAsyncResult BeginAcceptPooledConnection(IConnection connection, ref TimeoutHelper timeoutHelper,
            AsyncCallback callback, object state);
        protected abstract IConnection EndAcceptPooledConnection(IAsyncResult result);

        protected abstract TimeoutException CreateNewConnectionTimeoutException(TimeSpan timeout, TimeoutException innerException);

        public IAsyncResult BeginEstablishConnection(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new EstablishConnectionAsyncResult(this, timeout, callback, state);
        }

        public IConnection EndEstablishConnection(IAsyncResult result)
        {
            return EstablishConnectionAsyncResult.End(result);
        }

        IConnection TakeConnection(TimeSpan timeout)
        {
            return this.connectionPool.TakeConnection(null, this.via, timeout, out this.connectionKey);
        }

        public IConnection EstablishConnection(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            IConnection localRawConnection = null;
            IConnection localUpgradedConnection = null;
            bool localIsConnectionFromPool = true;

            EventTraceActivity localEventTraceActivity = this.EventTraceActivity;
            if (TD.EstablishConnectionStartIsEnabled())
            {
                TD.EstablishConnectionStart(localEventTraceActivity,
                                            this.via != null ? this.via.AbsoluteUri : string.Empty);
            }

            // first try and use a connection from our pool (and use it if we successfully receive an ACK)
            while (localIsConnectionFromPool)
            {
                localRawConnection = this.TakeConnection(timeoutHelper.RemainingTime());
                if (localRawConnection == null)
                {
                    localIsConnectionFromPool = false;
                }
                else
                {
                    bool preambleSuccess = false;
                    try
                    {
                        localUpgradedConnection = AcceptPooledConnection(localRawConnection, ref timeoutHelper);
                        preambleSuccess = true;
                        break;
                    }
                    catch (CommunicationException e)
                    {
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                        // CommmunicationException is ok since it was a cached connection of unknown state
                    }
                    catch (TimeoutException e)
                    {
                        if (TD.OpenTimeoutIsEnabled())
                        {
                            TD.OpenTimeout(e.Message);
                        }
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                        // ditto for TimeoutException
                    }
                    finally
                    {
                        if (!preambleSuccess)
                        {
                            if (TD.ConnectionPoolPreambleFailedIsEnabled())
                            {
                                TD.ConnectionPoolPreambleFailed(localEventTraceActivity);
                            }

                            if (DiagnosticUtility.ShouldTraceInformation)
                            {
                                TraceUtility.TraceEvent(
                                    TraceEventType.Information,
                                    TraceCode.FailedAcceptFromPool,
                                    SR.GetString(
                                        SR.TraceCodeFailedAcceptFromPool,
                                        timeoutHelper.RemainingTime()));
                            }

                            // This cannot throw TimeoutException since isConnectionStillGood is false (doesn't attempt a Close).
                            this.connectionPool.ReturnConnection(connectionKey, localRawConnection, false, TimeSpan.Zero);
                        }
                    }
                }
            }

            // if there isn't anything in the pool, we need to use a new connection
            if (!localIsConnectionFromPool)
            {
                bool success = false;
                TimeSpan connectTimeout = timeoutHelper.RemainingTime();
                try
                {
                    try
                    {
                        localRawConnection = this.connectionInitiator.Connect(this.via, connectTimeout);
                    }
                    catch (TimeoutException e)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateNewConnectionTimeoutException(
                            connectTimeout, e));
                    }

                    this.connectionInitiator = null;
                    localUpgradedConnection = AcceptPooledConnection(localRawConnection, ref timeoutHelper);
                    success = true;
                }
                finally
                {
                    if (!success)
                    {
                        connectionKey = null;
                        if (localRawConnection != null)
                        {
                            localRawConnection.Abort();
                        }
                    }
                }
            }

            SnapshotConnection(localUpgradedConnection, localRawConnection, localIsConnectionFromPool);

            if (TD.EstablishConnectionStopIsEnabled())
            {
                TD.EstablishConnectionStop(localEventTraceActivity);
            }

            return localUpgradedConnection;
        }

        void SnapshotConnection(IConnection upgradedConnection, IConnection rawConnection, bool isConnectionFromPool)
        {
            lock (ThisLock)
            {
                if (closed)
                {
                    upgradedConnection.Abort();

                    // cleanup our pool if necessary
                    if (isConnectionFromPool)
                    {
                        this.connectionPool.ReturnConnection(this.connectionKey, rawConnection, false, TimeSpan.Zero);
                    }

                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new CommunicationObjectAbortedException(
                        SR.GetString(SR.OperationAbortedDuringConnectionEstablishment, this.via)));
                }
                else
                {
                    this.upgradedConnection = upgradedConnection;
                    this.rawConnection = rawConnection;
                    this.isConnectionFromPool = isConnectionFromPool;
                }
            }
        }

        public void Abort()
        {
            ReleaseConnection(true, TimeSpan.Zero);
        }

        public void Close(TimeSpan timeout)
        {
            ReleaseConnection(false, timeout);
        }

        void ReleaseConnection(bool abort, TimeSpan timeout)
        {
            string localConnectionKey;
            IConnection localUpgradedConnection;
            IConnection localRawConnection;

            lock (ThisLock)
            {
                this.closed = true;
                localConnectionKey = this.connectionKey;
                localUpgradedConnection = this.upgradedConnection;
                localRawConnection = this.rawConnection;

                this.upgradedConnection = null;
                this.rawConnection = null;
            }

            if (localUpgradedConnection == null)
            {
                return;
            }

            try
            {
                if (this.isConnectionFromPool)
                {
                    this.connectionPool.ReturnConnection(localConnectionKey, localRawConnection, !abort, timeout);
                }
                else
                {
                    if (abort)
                    {
                        localUpgradedConnection.Abort();
                    }
                    else
                    {
                        this.connectionPool.AddConnection(localConnectionKey, localRawConnection, timeout);
                    }
                }
            }
            catch (CommunicationException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                localUpgradedConnection.Abort();
            }
        }

        class EstablishConnectionAsyncResult : AsyncResult
        {
            ConnectionPoolHelper parent;
            TimeoutHelper timeoutHelper;
            IConnection currentConnection;
            IConnection rawConnection;
            bool newConnection;
            bool cleanupConnection;
            TimeSpan connectTimeout;
            static AsyncCallback onConnect;
            static AsyncCallback onProcessConnection = Fx.ThunkCallback(new AsyncCallback(OnProcessConnection));
            EventTraceActivity eventTraceActivity;

            public EstablishConnectionAsyncResult(ConnectionPoolHelper parent,
                TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.parent = parent;
                this.timeoutHelper = new TimeoutHelper(timeout);

                bool success = false;
                bool completeSelf = false;
                try
                {
                    completeSelf = Begin();
                    success = true;
                }
                finally
                {
                    if (!success)
                    {
                        Cleanup();
                    }
                }

                if (completeSelf)
                {
                    Cleanup();
                    base.Complete(true);
                }
            }

            EventTraceActivity EventTraceActivity
            {
                get
                {
                    if (this.eventTraceActivity == null)
                    {
                        this.eventTraceActivity = new EventTraceActivity();
                    }
                    return this.eventTraceActivity;
                }
            }

            public static IConnection End(IAsyncResult result)
            {
                EstablishConnectionAsyncResult thisPtr = AsyncResult.End<EstablishConnectionAsyncResult>(result);

                if (TD.EstablishConnectionStopIsEnabled())
                {
                    TD.EstablishConnectionStop(thisPtr.EventTraceActivity);
                }

                return thisPtr.currentConnection;
            }

            bool Begin()
            {
                if (TD.EstablishConnectionStartIsEnabled())
                {
                    TD.EstablishConnectionStart(this.EventTraceActivity, this.parent.connectionKey);
                }

                IConnection connection = parent.TakeConnection(timeoutHelper.RemainingTime());

                TrackConnection(connection);

                // first try and use a connection from our pool
                bool openingFromPool;
                if (OpenUsingConnectionPool(out openingFromPool))
                {
                    return true;
                }

                if (openingFromPool)
                {
                    return false;
                }
                else
                {
                    // if there isn't anything in the pool, we need to use a new connection
                    return OpenUsingNewConnection();
                }
            }

            bool OpenUsingConnectionPool(out bool openingFromPool)
            {
                openingFromPool = true;
                while (this.currentConnection != null)
                {
                    bool snapshotCollection = false;
                    try
                    {
                        if (ProcessConnection())
                        {
                            snapshotCollection = true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch (CommunicationException e)
                    {
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                        // CommunicationException is allowed for cached channels, as the connection
                        // could be stale
                        Cleanup(); // remove residual state
                    }
                    catch (TimeoutException e)
                    {
                        if (TD.OpenTimeoutIsEnabled())
                        {
                            TD.OpenTimeout(e.Message);
                        }
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                        // ditto for TimeoutException
                        Cleanup(); // remove residual state
                    }

                    if (snapshotCollection) // connection succeeded. Snapshot and return
                    {
                        SnapshotConnection();
                        return true;
                    }

                    // previous connection failed, try again
                    IConnection connection = parent.TakeConnection(timeoutHelper.RemainingTime());

                    TrackConnection(connection);
                }

                openingFromPool = false;
                return false;
            }

            bool OpenUsingNewConnection()
            {
                this.newConnection = true;
                IAsyncResult result;

                try
                {
                    this.connectTimeout = timeoutHelper.RemainingTime();

                    if (onConnect == null)
                    {
                        onConnect = Fx.ThunkCallback(new AsyncCallback(OnConnect));
                    }

                    result = parent.connectionInitiator.BeginConnect(
                         parent.via, this.connectTimeout, onConnect, this);
                }
                catch (TimeoutException e)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        parent.CreateNewConnectionTimeoutException(connectTimeout, e));
                }

                if (!result.CompletedSynchronously)
                {
                    return false;
                }

                return HandleConnect(result);
            }

            bool HandleConnect(IAsyncResult connectResult)
            {
                try
                {
                    TrackConnection(parent.connectionInitiator.EndConnect(connectResult));
                }
                catch (TimeoutException e)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        parent.CreateNewConnectionTimeoutException(connectTimeout, e));
                }

                if (ProcessConnection())
                {
                    // success. Snapshot and return
                    SnapshotConnection();
                    return true;
                }
                else
                {
                    return false;
                }
            }

            bool ProcessConnection()
            {
                IAsyncResult result = parent.BeginAcceptPooledConnection(this.rawConnection,
                    ref timeoutHelper, onProcessConnection, this);
                if (!result.CompletedSynchronously)
                {
                    return false;
                }

                return HandleProcessConnection(result);
            }

            bool HandleProcessConnection(IAsyncResult result)
            {
                this.currentConnection = parent.EndAcceptPooledConnection(result);
                this.cleanupConnection = false;
                return true;
            }

            void SnapshotConnection()
            {
                parent.SnapshotConnection(this.currentConnection, this.rawConnection, !this.newConnection);
            }

            void TrackConnection(IConnection connection)
            {
                this.cleanupConnection = true;
                this.rawConnection = connection;
                this.currentConnection = connection;
            }

            void Cleanup()
            {
                if (this.cleanupConnection)
                {
                    if (this.newConnection)
                    {
                        if (this.currentConnection != null)
                        {
                            this.currentConnection.Abort();
                            this.currentConnection = null;
                        }
                    }
                    else if (this.rawConnection != null)
                    {
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            TraceUtility.TraceEvent(
                                TraceEventType.Information,
                                TraceCode.FailedAcceptFromPool,
                                SR.GetString(
                                    SR.TraceCodeFailedAcceptFromPool,
                                    this.timeoutHelper.RemainingTime()));
                        }

                        // This cannot throw TimeoutException since isConnectionStillGood is false (doesn't attempt a Close).
                        parent.connectionPool.ReturnConnection(parent.connectionKey, this.rawConnection,
                            false, timeoutHelper.RemainingTime());
                        this.currentConnection = null;
                        this.rawConnection = null;
                    }

                    this.cleanupConnection = false;
                }
            }

            static void OnConnect(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                EstablishConnectionAsyncResult thisPtr = (EstablishConnectionAsyncResult)result.AsyncState;

                Exception completionException = null;
                bool completeSelf;
                try
                {
                    completeSelf = thisPtr.HandleConnect(result);
                }
#pragma warning suppress 56500 // [....], transferring exception to another thread
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completeSelf = true;
                    completionException = e;
                }

                if (completeSelf)
                {
                    thisPtr.Cleanup();
                    thisPtr.Complete(false, completionException);
                }
            }

            static void OnProcessConnection(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                EstablishConnectionAsyncResult thisPtr = (EstablishConnectionAsyncResult)result.AsyncState;

                Exception completionException = null;
                bool completeSelf;
                try
                {
                    bool snapshotCollection = false;
                    try
                    {
                        completeSelf = thisPtr.HandleProcessConnection(result);
                        if (completeSelf)
                        {
                            snapshotCollection = true;
                        }
                    }
                    catch (CommunicationException communicationException)
                    {
                        if (!thisPtr.newConnection) // CommunicationException is ok from our cache
                        {
                            DiagnosticUtility.TraceHandledException(communicationException, TraceEventType.Information);                            
                            thisPtr.Cleanup();
                            completeSelf = thisPtr.Begin();
                        }
                        else
                        {
                            completeSelf = true;
                            completionException = communicationException;
                        }
                    }
                    catch (TimeoutException timeoutException)
                    {
                        if (!thisPtr.newConnection) // TimeoutException is ok from our cache
                        {
                            if (TD.OpenTimeoutIsEnabled())
                            {
                                TD.OpenTimeout(timeoutException.Message);
                            }
                            DiagnosticUtility.TraceHandledException(timeoutException, TraceEventType.Information);                            
                            thisPtr.Cleanup();
                            completeSelf = thisPtr.Begin();
                        }
                        else
                        {
                            completeSelf = true;
                            completionException = timeoutException;
                        }
                    }

                    if (snapshotCollection)
                    {
                        thisPtr.SnapshotConnection();
                    }
                }
#pragma warning suppress 56500 // [....], transferring exception to another thread
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completeSelf = true;
                    completionException = e;
                }

                if (completeSelf)
                {
                    thisPtr.Cleanup();
                    thisPtr.Complete(false, completionException);
                }
            }
        }
    }
}


