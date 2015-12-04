//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel.Diagnostics;
    using System.Threading;
    using System.ServiceModel.Diagnostics.Application;

    abstract class IdlingCommunicationPool<TKey, TItem>
        : CommunicationPool<TKey, TItem>
        where TKey : class
        where TItem : class
    {
        TimeSpan idleTimeout;
        TimeSpan leaseTimeout;

        protected IdlingCommunicationPool(int maxCount, TimeSpan idleTimeout, TimeSpan leaseTimeout)
            : base(maxCount)
        {
            this.idleTimeout = idleTimeout;
            this.leaseTimeout = leaseTimeout;
        }

        public TimeSpan IdleTimeout
        {
            get { return this.idleTimeout; }
        }

        protected TimeSpan LeaseTimeout
        {
            get { return this.leaseTimeout; }
        }

        protected override void CloseItemAsync(TItem item, TimeSpan timeout)
        {
            // Default behavior is [....]. Derived classes can override.
            this.CloseItem(item, timeout);
        }

        protected override EndpointConnectionPool CreateEndpointConnectionPool(TKey key)
        {
            if (idleTimeout != TimeSpan.MaxValue || leaseTimeout != TimeSpan.MaxValue)
            {
                return new IdleTimeoutEndpointConnectionPool(this, key);
            }
            else
            {
                return base.CreateEndpointConnectionPool(key);
            }
        }

        protected class IdleTimeoutEndpointConnectionPool : EndpointConnectionPool
        {
            IdleTimeoutIdleConnectionPool connections;

            public IdleTimeoutEndpointConnectionPool(IdlingCommunicationPool<TKey, TItem> parent, TKey key)
                : base(parent, key)
            {
                this.connections = new IdleTimeoutIdleConnectionPool(this, this.ThisLock);
            }

            protected override IdleConnectionPool GetIdleConnectionPool()
            {
                return this.connections;
            }

            protected override void AbortItem(TItem item)
            {
                this.connections.OnItemClosing(item);
                base.AbortItem(item);
            }

            protected override void CloseItemAsync(TItem item, TimeSpan timeout)
            {
                this.connections.OnItemClosing(item);
                base.CloseItemAsync(item, timeout);
            }

            protected override void CloseItem(TItem item, TimeSpan timeout)
            {
                this.connections.OnItemClosing(item);
                base.CloseItem(item, timeout);
            }

            public override void Prune(List<TItem> itemsToClose)
            {
                if (this.connections != null)
                {
                    this.connections.Prune(itemsToClose, false);
                }
            }

            protected class IdleTimeoutIdleConnectionPool : PoolIdleConnectionPool
            {
                // for performance reasons we don't just blindly start a timer up to clean up 
                // idle connections. However, if we're above a certain threshold of connections
                const int timerThreshold = 1;

                IdleTimeoutEndpointConnectionPool parent;
                TimeSpan idleTimeout;
                TimeSpan leaseTimeout;
                IOThreadTimer idleTimer;
                static Action<object> onIdle;
                object thisLock;
                Exception pendingException;

                // Note that Take/Add/Return are already synchronized by ThisLock, so we don't need an extra
                // lock around our Dictionary access
                Dictionary<TItem, IdlingConnectionSettings> connectionMapping;

                public IdleTimeoutIdleConnectionPool(IdleTimeoutEndpointConnectionPool parent, object thisLock)
                    : base(parent.Parent.MaxIdleConnectionPoolCount)
                {
                    this.parent = parent;
                    IdlingCommunicationPool<TKey, TItem> idlingCommunicationPool = ((IdlingCommunicationPool<TKey, TItem>)parent.Parent);
                    this.idleTimeout = idlingCommunicationPool.idleTimeout;
                    this.leaseTimeout = idlingCommunicationPool.leaseTimeout;
                    this.thisLock = thisLock;
                    this.connectionMapping = new Dictionary<TItem, IdlingConnectionSettings>();
                }

                public override bool Add(TItem connection)
                {
                    this.ThrowPendingException();

                    bool result = base.Add(connection);
                    if (result)
                    {
                        this.connectionMapping.Add(connection, new IdlingConnectionSettings());
                        StartTimerIfNecessary();
                    }
                    return result;
                }

                public override bool Return(TItem connection)
                {
                    this.ThrowPendingException();

                    if (!this.connectionMapping.ContainsKey(connection))
                    {
                        return false;
                    }

                    bool result = base.Return(connection);
                    if (result)
                    {
                        this.connectionMapping[connection].LastUsage = DateTime.UtcNow;
                        StartTimerIfNecessary();
                    }
                    return result;
                }

                public override TItem Take(out bool closeItem)
                {
                    this.ThrowPendingException();

                    DateTime now = DateTime.UtcNow;
                    TItem item = base.Take(out closeItem);

                    if (!closeItem)
                    {
                        closeItem = IdleOutConnection(item, now);
                    }
                    return item;
                }

                public void OnItemClosing(TItem connection)
                {
                    this.ThrowPendingException();

                    lock (thisLock)
                    {
                        this.connectionMapping.Remove(connection);
                    }
                }

                void CancelTimer()
                {
                    if (this.idleTimer != null)
                    {
                        this.idleTimer.Cancel();
                    }
                }

                void StartTimerIfNecessary()
                {
                    if (this.Count > timerThreshold)
                    {
                        if (idleTimer == null)
                        {
                            if (onIdle == null)
                            {
                                onIdle = new Action<object>(OnIdle);
                            }

                            idleTimer = new IOThreadTimer(onIdle, this, false);
                        }

                        idleTimer.Set(idleTimeout);
                    }
                }

                static void OnIdle(object state)
                {
                    IdleTimeoutIdleConnectionPool pool = (IdleTimeoutIdleConnectionPool)state;
                    pool.OnIdle();
                }

                void OnIdle()
                {
                    List<TItem> itemsToClose = new List<TItem>();
                    lock (thisLock)
                    {
                        try
                        {
                            this.Prune(itemsToClose, true);
                        }
                        catch (Exception e)
                        {
                            if (Fx.IsFatal(e))
                            {
                                throw;
                            }
                            this.pendingException = e;
                            this.CancelTimer();
                        }
                    }

                    // allocate half the idle timeout for our g----ful shutdowns
                    TimeoutHelper timeoutHelper = new TimeoutHelper(TimeoutHelper.Divide(this.idleTimeout, 2));
                    for (int i = 0; i < itemsToClose.Count; i++)
                    {
                        parent.CloseIdleConnection(itemsToClose[i], timeoutHelper.RemainingTime());
                    }
                }

                public void Prune(List<TItem> itemsToClose, bool calledFromTimer)
                {
                    if (!calledFromTimer)
                    {
                        this.ThrowPendingException();
                    }

                    if (this.Count == 0)
                        return;

                    DateTime now = DateTime.UtcNow;
                    bool setTimer = false;

                    lock (thisLock)
                    {
                        TItem[] connectionsCopy = new TItem[this.Count];
                        for (int i = 0; i < connectionsCopy.Length; i++)
                        {
                            bool closeItem;
                            connectionsCopy[i] = base.Take(out closeItem);
                            Fx.Assert(connectionsCopy[i] != null, "IdleConnections should only be modified under thisLock");
                            if (closeItem || IdleOutConnection(connectionsCopy[i], now))
                            {
                                itemsToClose.Add(connectionsCopy[i]);
                                connectionsCopy[i] = null;
                            }
                        }

                        for (int i = 0; i < connectionsCopy.Length; i++)
                        {
                            if (connectionsCopy[i] != null)
                            {
                                bool successfulReturn = base.Return(connectionsCopy[i]);
                                Fx.Assert(successfulReturn, "IdleConnections should only be modified under thisLock");
                            }
                        }

                        setTimer = (this.Count > 0);
                    }

                    if (calledFromTimer && setTimer)
                    {
                        idleTimer.Set(idleTimeout);
                    }
                }

                bool IdleOutConnection(TItem connection, DateTime now)
                {
                    if (connection == null)
                    {
                        return false;
                    }

                    bool result = false;
                    IdlingConnectionSettings idlingSettings = this.connectionMapping[connection];
                    if (now > (idlingSettings.LastUsage + this.idleTimeout))
                    {
                        TraceConnectionIdleTimeoutExpired();
                        result = true;
                    }
                    else if (now - idlingSettings.CreationTime >= this.leaseTimeout)
                    {
                        TraceConnectionLeaseTimeoutExpired();
                        result = true;
                    }

                    return result;
                }

                void ThrowPendingException()
                {
                    if (this.pendingException != null)
                    {
                        lock (thisLock)
                        {
                            if (this.pendingException != null)
                            {
                                Exception exceptionToThrow = this.pendingException;
                                this.pendingException = null;
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exceptionToThrow);
                            }
                        }
                    }
                }

                void TraceConnectionLeaseTimeoutExpired()
                {
                    if (TD.LeaseTimeoutIsEnabled())
                    {
                        TD.LeaseTimeout(SR.GetString(SR.TraceCodeConnectionPoolLeaseTimeoutReached, this.leaseTimeout), this.parent.Key.ToString());
                    }
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Information,
                            TraceCode.ConnectionPoolLeaseTimeoutReached,
                            SR.GetString(SR.TraceCodeConnectionPoolLeaseTimeoutReached, this.leaseTimeout),
                            this);
                    }
                }

                void TraceConnectionIdleTimeoutExpired()
                {
                    if (TD.IdleTimeoutIsEnabled())
                    {
                        TD.IdleTimeout(SR.GetString(SR.TraceCodeConnectionPoolIdleTimeoutReached, this.idleTimeout), this.parent.Key.ToString());
                    }
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Information,
                            TraceCode.ConnectionPoolIdleTimeoutReached,
                            SR.GetString(SR.TraceCodeConnectionPoolIdleTimeoutReached, this.idleTimeout),
                            this);
                    }
                }

                class IdlingConnectionSettings
                {
                    DateTime creationTime;
                    DateTime lastUsage;

                    public IdlingConnectionSettings()
                    {
                        this.creationTime = DateTime.UtcNow;
                        this.lastUsage = this.creationTime;
                    }

                    public DateTime CreationTime
                    {
                        get { return this.creationTime; }
                    }

                    public DateTime LastUsage
                    {
                        get { return this.lastUsage; }
                        set { this.lastUsage = value; }
                    }
                }
            }
        }
    }
}
