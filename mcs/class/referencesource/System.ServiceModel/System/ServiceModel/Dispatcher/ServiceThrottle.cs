//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;

    interface ISessionThrottleNotification
    {
        void ThrottleAcquired();
    }

    public sealed class ServiceThrottle
    {
        internal const int DefaultMaxConcurrentCalls = 16;
        internal const int DefaultMaxConcurrentSessions = 100;
        internal static int DefaultMaxConcurrentCallsCpuCount = DefaultMaxConcurrentCalls * OSEnvironmentHelper.ProcessorCount;
        internal static int DefaultMaxConcurrentSessionsCpuCount = DefaultMaxConcurrentSessions * OSEnvironmentHelper.ProcessorCount;

        FlowThrottle calls;
        FlowThrottle sessions;
        QuotaThrottle dynamic;
        FlowThrottle instanceContexts;

        ServiceHostBase host;
        ServicePerformanceCountersBase servicePerformanceCounters;
        bool isActive;
        object thisLock = new object();

        internal ServiceThrottle(ServiceHostBase host)
        {
            if (!((host != null)))
            {
                Fx.Assert("ServiceThrottle.ServiceThrottle: (host != null)");
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("host");
            }
            this.host = host;
            this.MaxConcurrentCalls = ServiceThrottle.DefaultMaxConcurrentCallsCpuCount;
            this.MaxConcurrentSessions = ServiceThrottle.DefaultMaxConcurrentSessionsCpuCount;

            this.isActive = true;
        }

        FlowThrottle Calls
        {
            get
            {
                lock (this.ThisLock)
                {
                    if (this.calls == null)
                    {
                        this.calls = new FlowThrottle(this.GotCall, ServiceThrottle.DefaultMaxConcurrentCallsCpuCount,
                                                      ServiceThrottle.MaxConcurrentCallsPropertyName, ServiceThrottle.MaxConcurrentCallsConfigName);
                        this.calls.SetRatio(this.RatioCallsToken);
                    }
                    return this.calls;
                }
            }
        }

        FlowThrottle Sessions
        {
            get
            {
                lock (this.ThisLock)
                {
                    if (this.sessions == null)
                    {
                        this.sessions = new FlowThrottle(this.GotSession, ServiceThrottle.DefaultMaxConcurrentSessionsCpuCount,
                                                         ServiceThrottle.MaxConcurrentSessionsPropertyName, ServiceThrottle.MaxConcurrentSessionsConfigName);
                        this.sessions.SetRatio(this.RatioSessionsToken);
                    }
                    return this.sessions;
                }
            }
        }

        QuotaThrottle Dynamic
        {
            get
            {
                lock (this.ThisLock)
                {
                    if (this.dynamic == null)
                    {
                        this.dynamic = new QuotaThrottle(this.GotDynamic, new object());
                        this.dynamic.Owner = "ServiceHost";
                    }
                    this.UpdateIsActive();
                    return this.dynamic;
                }
            }
        }

        internal int ManualFlowControlLimit
        {
            get { return this.Dynamic.Limit; }
            set { this.Dynamic.SetLimit(value); }
        }

        const string MaxConcurrentCallsPropertyName = "MaxConcurrentCalls";
        const string MaxConcurrentCallsConfigName = "maxConcurrentCalls";
        public int MaxConcurrentCalls
        {
            get { return this.Calls.Capacity; }
            set
            {
                this.ThrowIfClosedOrOpened(MaxConcurrentCallsPropertyName);
                this.Calls.Capacity = value;
                this.UpdateIsActive();
                if (null != this.servicePerformanceCounters)
                {
                    this.servicePerformanceCounters.SetThrottleBase((int)ServicePerformanceCounters.PerfCounters.CallsPercentMaxCallsBase, this.Calls.Capacity);
                }
            }
        }

        const string MaxConcurrentSessionsPropertyName = "MaxConcurrentSessions";
        const string MaxConcurrentSessionsConfigName = "maxConcurrentSessions";
        public int MaxConcurrentSessions
        {
            get { return this.Sessions.Capacity; }
            set
            {
                this.ThrowIfClosedOrOpened(MaxConcurrentSessionsPropertyName);
                this.Sessions.Capacity = value;
                this.UpdateIsActive();
                if (null != this.servicePerformanceCounters)
                {
                    this.servicePerformanceCounters.SetThrottleBase((int)ServicePerformanceCounters.PerfCounters.SessionsPercentMaxSessionsBase, this.Sessions.Capacity);
                }
            }
        }

        const string MaxConcurrentInstancesPropertyName = "MaxConcurrentInstances";
        const string MaxConcurrentInstancesConfigName = "maxConcurrentInstances";
        public int MaxConcurrentInstances
        {
            get { return this.InstanceContexts.Capacity; }
            set
            {
                this.ThrowIfClosedOrOpened(MaxConcurrentInstancesPropertyName);
                this.InstanceContexts.Capacity = value;
                this.UpdateIsActive();
                if (null != this.servicePerformanceCounters)
                {
                    this.servicePerformanceCounters.SetThrottleBase((int)ServicePerformanceCounters.PerfCounters.InstancesPercentMaxInstancesBase, this.InstanceContexts.Capacity);
                }
            }
        }

        FlowThrottle InstanceContexts
        {
            get
            {
                lock (this.ThisLock)
                {
                    if (this.instanceContexts == null)
                    {
                        this.instanceContexts = new FlowThrottle(this.GotInstanceContext, Int32.MaxValue,
                                                                 ServiceThrottle.MaxConcurrentInstancesPropertyName, ServiceThrottle.MaxConcurrentInstancesConfigName);
                        this.instanceContexts.SetRatio(this.RatioInstancesToken);
                        if (this.servicePerformanceCounters != null)
                        {
                            InitializeInstancePerfCounterSettings();
                        }
                    }
                    return this.instanceContexts;
                }
            }
        }

        internal bool IsActive
        {
            get { return this.isActive; }
        }

        internal object ThisLock
        {
            get { return this.thisLock; }
        }

        internal void SetServicePerformanceCounters(ServicePerformanceCountersBase counters)
        {
            this.servicePerformanceCounters = counters;
            //instance throttle is created through the behavior, set the perf counter callbacks if initialized
            if (this.instanceContexts != null)
            {
                InitializeInstancePerfCounterSettings();
            }

            //this.calls and this.sessions throttles are created by the constructor. Set the perf counter callbacks
            InitializeCallsPerfCounterSettings();
            InitializeSessionsPerfCounterSettings();
        }

        void InitializeInstancePerfCounterSettings()
        {
            Fx.Assert(this.instanceContexts != null, "Expect instanceContext to be initialized");
            Fx.Assert(this.servicePerformanceCounters != null, "expect servicePerformanceCounters to be set");
            this.instanceContexts.SetAcquired(this.AcquiredInstancesToken);
            this.instanceContexts.SetReleased(this.ReleasedInstancesToken);
            this.instanceContexts.SetRatio(this.RatioInstancesToken);
            this.servicePerformanceCounters.SetThrottleBase((int)ServicePerformanceCounters.PerfCounters.InstancesPercentMaxInstancesBase, this.instanceContexts.Capacity);
        }

        void InitializeCallsPerfCounterSettings()
        {
            Fx.Assert(this.calls != null, "Expect calls to be initialized");
            Fx.Assert(this.servicePerformanceCounters != null, "expect servicePerformanceCounters to be set");
            this.calls.SetAcquired(this.AcquiredCallsToken);
            this.calls.SetReleased(this.ReleasedCallsToken);
            this.calls.SetRatio(this.RatioCallsToken);
            this.servicePerformanceCounters.SetThrottleBase((int)ServicePerformanceCounters.PerfCounters.CallsPercentMaxCallsBase, this.calls.Capacity);
        }

        void InitializeSessionsPerfCounterSettings()
        {
            Fx.Assert(this.sessions != null, "Expect sessions to be initialized");
            Fx.Assert(this.servicePerformanceCounters != null, "expect servicePerformanceCounters to be set");
            this.sessions.SetAcquired(this.AcquiredSessionsToken);
            this.sessions.SetReleased(this.ReleasedSessionsToken);
            this.sessions.SetRatio(this.RatioSessionsToken);
            this.servicePerformanceCounters.SetThrottleBase((int)ServicePerformanceCounters.PerfCounters.SessionsPercentMaxSessionsBase, this.sessions.Capacity);
        }

        bool PrivateAcquireCall(ChannelHandler channel)
        {
            return (this.calls == null) || this.calls.Acquire(channel);
        }

        bool PrivateAcquireSessionListenerHandler(ListenerHandler listener)
        {
            if ((this.sessions != null) && (listener.Channel != null) && (listener.Channel.Throttle == null))
            {
                listener.Channel.Throttle = this;
                return this.sessions.Acquire(listener);
            }
            else
            {
                return true;
            }
        }

        bool PrivateAcquireSession(ISessionThrottleNotification source)
        {
            return (this.sessions == null || this.sessions.Acquire(source));
        }

        bool PrivateAcquireDynamic(ChannelHandler channel)
        {
            return (this.dynamic == null) || this.dynamic.Acquire(channel);
        }

        bool PrivateAcquireInstanceContext(ChannelHandler channel)
        {
            if ((this.instanceContexts != null) && (channel.InstanceContext == null))
            {
                channel.InstanceContextServiceThrottle = this;
                return this.instanceContexts.Acquire(channel);
            }
            else
            {
                return true;
            }
        }

        internal bool AcquireCall(ChannelHandler channel)
        {
            lock (this.ThisLock)
            {
                return (this.PrivateAcquireCall(channel));
            }
        }

        internal bool AcquireInstanceContextAndDynamic(ChannelHandler channel, bool acquireInstanceContextThrottle)
        {
            lock (this.ThisLock)
            {
                if (!acquireInstanceContextThrottle)
                {
                    return this.PrivateAcquireDynamic(channel);
                }
                else
                {
                    return (this.PrivateAcquireInstanceContext(channel) &&
                            this.PrivateAcquireDynamic(channel));
                }
            }
        }

        internal bool AcquireSession(ISessionThrottleNotification source)
        {
            lock (this.ThisLock)
            {
                return this.PrivateAcquireSession(source);
            }
        }

        internal bool AcquireSession(ListenerHandler listener)
        {
            lock (this.ThisLock)
            {
                return this.PrivateAcquireSessionListenerHandler(listener);
            }
        }

        void GotCall(object state)
        {
            ChannelHandler channel = (ChannelHandler)state;

            lock (this.ThisLock)
            {
                channel.ThrottleAcquiredForCall();
            }
        }

        void GotDynamic(object state)
        {
            ((ChannelHandler)state).ThrottleAcquired();
        }

        void GotInstanceContext(object state)
        {
            ChannelHandler channel = (ChannelHandler)state;

            lock (this.ThisLock)
            {
                if (this.PrivateAcquireDynamic(channel))
                    channel.ThrottleAcquired();
            }
        }

        void GotSession(object state)
        {
            ((ISessionThrottleNotification)state).ThrottleAcquired();
        }

        internal void DeactivateChannel()
        {
            if (this.isActive)
            {
                if (this.sessions != null)
                    this.sessions.Release();
            }
        }

        internal void DeactivateCall()
        {
            if (this.isActive)
            {
                if (this.calls != null)
                    this.calls.Release();
            }
        }

        internal void DeactivateInstanceContext()
        {
            if (this.isActive)
            {
                if (this.instanceContexts != null)
                {
                    this.instanceContexts.Release();
                }
            }
        }

        internal int IncrementManualFlowControlLimit(int incrementBy)
        {
            return this.Dynamic.IncrementLimit(incrementBy);
        }

        void ThrowIfClosedOrOpened(string memberName)
        {
            if (this.host.State == CommunicationState.Opened)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxImmutableThrottle1, memberName)));
            }
            else
            {
                this.host.ThrowIfClosedOrOpened();
            }
        }

        void UpdateIsActive()
        {
            this.isActive = ((this.dynamic != null) ||
                             ((this.calls != null) && (this.calls.Capacity != Int32.MaxValue)) ||
                             ((this.sessions != null) && (this.sessions.Capacity != Int32.MaxValue)) ||
                             ((this.instanceContexts != null) && (this.instanceContexts.Capacity != Int32.MaxValue)));
        }

        internal void AcquiredCallsToken()
        {
            this.servicePerformanceCounters.IncrementThrottlePercent((int)ServicePerformanceCounters.PerfCounters.CallsPercentMaxCalls);
        }

        internal void ReleasedCallsToken()
        {
            this.servicePerformanceCounters.DecrementThrottlePercent((int)ServicePerformanceCounters.PerfCounters.CallsPercentMaxCalls);
        }

        internal void RatioCallsToken(int count)
        {
            if (TD.ConcurrentCallsRatioIsEnabled())
            {
                TD.ConcurrentCallsRatio(count, this.MaxConcurrentCalls);
            }
        }

        internal void AcquiredInstancesToken()
        {
            this.servicePerformanceCounters.IncrementThrottlePercent((int)ServicePerformanceCounters.PerfCounters.InstancesPercentMaxInstances);
        }

        internal void ReleasedInstancesToken()
        {
            this.servicePerformanceCounters.DecrementThrottlePercent((int)ServicePerformanceCounters.PerfCounters.InstancesPercentMaxInstances);
        }

        internal void RatioInstancesToken(int count)
        {
            if (TD.ConcurrentInstancesRatioIsEnabled())
            {
                TD.ConcurrentInstancesRatio(count, this.MaxConcurrentInstances);
            }
        }

        internal void AcquiredSessionsToken()
        {
            this.servicePerformanceCounters.IncrementThrottlePercent((int)ServicePerformanceCounters.PerfCounters.SessionsPercentMaxSessions);
        }

        internal void ReleasedSessionsToken()
        {
            this.servicePerformanceCounters.DecrementThrottlePercent((int)ServicePerformanceCounters.PerfCounters.SessionsPercentMaxSessions);
        }

        internal void RatioSessionsToken(int count)
        {
            if (TD.ConcurrentSessionsRatioIsEnabled())
            {
                TD.ConcurrentSessionsRatio(count, this.MaxConcurrentSessions);
            }
        }
    }
}
