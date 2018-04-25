//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Runtime.Hosting
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Workflow.Activities;
    using System.Workflow.ComponentModel;

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class ChannelManagerService : WorkflowRuntimeService
    {
        const string IdleTimeoutSetting = "idleTimeout";
        const string InfiniteTimeSpanValue = "infinite";
        const string LeaseTimeoutSetting = "leaseTimeout";
        const string MaxIdleChannelsPerEndpointSetting = "maxIdleChannelsPerEndpoint";

        ChannelManager channelManager;

        bool closed;
        IList<ServiceEndpoint> codeEndpoints;
        ChannelPoolSettings settings;

        public ChannelManagerService()
            : this(new ChannelPoolSettings(), new List<ServiceEndpoint>())
        {
        }

        public ChannelManagerService(ChannelPoolSettings settings)
            : this(settings, new List<ServiceEndpoint>())
        {
        }

        public ChannelManagerService(IList<ServiceEndpoint> endpoints)
            : this(new ChannelPoolSettings(), endpoints)
        {
        }

        public ChannelManagerService(ChannelPoolSettings settings, IList<ServiceEndpoint> endpoints)
        {
            if (settings == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("settings");
            }

            if (endpoints == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoints");
            }

            this.settings = settings;
            this.codeEndpoints = endpoints;
        }

        public ChannelManagerService(NameValueCollection parameters)
        {
            if (parameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");
            }

            ChannelPoolSettings channelPoolSettings = new ChannelPoolSettings();

            foreach (string key in parameters.Keys)
            {
                if (string.IsNullOrEmpty(key))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(
                        SR2.GetString(SR2.Error_UnknownConfigurationParameter, key), "parameters");
                }
                else if (key.Equals(ChannelManagerService.IdleTimeoutSetting, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        channelPoolSettings.IdleTimeout =
                            ConvertToTimeSpan(parameters[ChannelManagerService.IdleTimeoutSetting]);
                    }
                    catch (FormatException ex)
                    {
                        ArgumentException exception = new ArgumentException(
                            SR2.GetString(SR2.Error_InvalidIdleTimeout, parameters[ChannelManagerService.IdleTimeoutSetting]),
                            "parameters",
                            ex);

                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
                    }
                }
                else if (key.Equals(ChannelManagerService.LeaseTimeoutSetting, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        channelPoolSettings.LeaseTimeout =
                            ConvertToTimeSpan(parameters[ChannelManagerService.LeaseTimeoutSetting]);
                    }
                    catch (FormatException ex)
                    {
                        ArgumentException exception = new ArgumentException(
                            SR2.GetString(SR2.Error_InvalidLeaseTimeout, parameters[ChannelManagerService.LeaseTimeoutSetting]),
                            "parameters",
                            ex);

                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
                    }
                }
                else if (key.Equals(ChannelManagerService.MaxIdleChannelsPerEndpointSetting, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        channelPoolSettings.MaxOutboundChannelsPerEndpoint =
                            Convert.ToInt32(parameters[ChannelManagerService.MaxIdleChannelsPerEndpointSetting], System.Globalization.CultureInfo.CurrentCulture);
                    }
                    catch (FormatException ex)
                    {
                        ArgumentException exception = new ArgumentException(
                            SR2.GetString(SR2.Error_InvalidMaxIdleChannelsPerEndpoint, parameters[ChannelManagerService.MaxIdleChannelsPerEndpointSetting]),
                            "parameters",
                            ex);

                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
                    }
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(
                        SR2.GetString(SR2.Error_UnknownConfigurationParameter, key), "parameters");
                }
            }

            this.settings = channelPoolSettings;
            this.codeEndpoints = new List<ServiceEndpoint>();
        }

        protected internal override void Start()
        {
            base.Start();

            this.channelManager = new ChannelManager(this.settings, this.codeEndpoints);
            this.closed = false;
        }

        protected internal override void Stop()
        {
            base.Stop();

            if (!this.closed && this.channelManager != null)
            {
                this.channelManager.Close();
            }

            this.closed = true;
        }

        internal static void ApplyLogicalChannelContext(LogicalChannel logicalChannel)
        {
            Fx.Assert(OperationContext.Current != null, "Can be called within a valid OperationContext Scope");

            WorkflowTrace.Host.TraceEvent(TraceEventType.Verbose, 0,
                "ChannelManagerService: updating context associated with logical channel {0}",
                logicalChannel.InstanceId);

            if (logicalChannel.Context != null)
            {
                new ContextMessageProperty(logicalChannel.Context).AddOrReplaceInMessageProperties(OperationContext.Current.OutgoingMessageProperties);
            }
        }

        internal static TransientChannelTicket CreateTransientChannel(LogicalChannel logicalChannel)
        {
            DiagnosticUtility.DebugAssert(logicalChannel != null, "logical channel cannot be null");

            ChannelFactory factory = null;
            IChannel channel = null;
            bool channelOpened = false;

            try
            {
                factory = ChannelManagerHelpers.CreateChannelFactory(logicalChannel.ConfigurationName, logicalChannel.ContractType);
                channel = ChannelManagerHelpers.CreateChannel(logicalChannel.ContractType, factory, logicalChannel.CustomAddress);
                channelOpened = true;
            }
            finally
            {
                if (!channelOpened)
                {
                    if (channel != null)
                    {
                        ChannelManagerHelpers.CloseCommunicationObject(channel);
                    }
                    if (factory != null)
                    {
                        ChannelManagerHelpers.CloseCommunicationObject(factory);
                    }
                }
            }

            return new TransientChannelTicket(channel, factory);
        }

        internal static ChannelTicket Take(ActivityExecutionContext executionContext, Guid workflowId, LogicalChannel logicalChannel)
        {
            if (executionContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("executionContext");
            }

            if (workflowId == Guid.Empty)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("workflowId",
                    SR2.GetString(SR2.Error_Cache_InvalidWorkflowId));
            }

            if (logicalChannel == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("logicalChannel");
            }

            ChannelManagerService channelManager = executionContext.GetService<ChannelManagerService>();

            ChannelTicket channel;
            if (channelManager != null)
            {
                channel = channelManager.TakeChannel(workflowId, logicalChannel);
            }
            else
            {
                channel = ChannelManagerService.CreateTransientChannel(logicalChannel);
            }

            return channel;
        }

        internal static void UpdateLogicalChannelContext(LogicalChannel logicalChannel)
        {
            Fx.Assert(OperationContext.Current != null, "Can be called from valid OperationContextScope");

            WorkflowTrace.Host.TraceEvent(TraceEventType.Verbose, 0,
                "ChannelManagerService: updating context associated with logical channel {0}",
                logicalChannel.InstanceId);

            ContextMessageProperty contextMessageProperty;
            MessageProperties properties = OperationContext.Current.IncomingMessageProperties;

            if (properties != null && ContextMessageProperty.TryGet(properties, out contextMessageProperty))
            {
                logicalChannel.Context = contextMessageProperty.Context;
            }
        }

        internal void ReturnChannel(PooledChannelTicket pooledChannel)
        {
            DiagnosticUtility.DebugAssert(pooledChannel != null, "pooled channel cannot be null");
            if (pooledChannel == null)
            {
                return;
            }

            WorkflowTrace.Host.TraceEvent(
                TraceEventType.Information, 0,
                "ChannelManagerService: return channel for workflow instance {0}, logical channel {1}",
                new object[] { pooledChannel.WorkflowId, pooledChannel.LogicalChannelId });

            this.channelManager.ReturnChannel(pooledChannel.ChannelPoolKey, pooledChannel.PooledChannel);
        }

        internal PooledChannelTicket TakeChannel(Guid workflowId, LogicalChannel logicalChannel)
        {
            if (this.closed)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR2.GetString(SR2.Error_CannotProvideChannel_ServiceStopped, logicalChannel.ConfigurationName, logicalChannel.CustomAddress)));
            }

            if (workflowId == Guid.Empty)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("workflowId",
                    SR2.GetString(SR2.Error_Cache_InvalidWorkflowId));
            }

            if (logicalChannel == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("logicalChannel");
            }

            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "ChannelManagerService: get channel for workflow instance {0}, logical channel {1}", new object[] { workflowId, logicalChannel.InstanceId });

            string endpointName = logicalChannel.ConfigurationName;
            Type contractType = logicalChannel.ContractType;
            string customAddress = logicalChannel.CustomAddress;

            ChannelPoolKey channelKey;
            ChannelManager.PooledChannel channel = this.channelManager.TakeChannel(endpointName, contractType, customAddress, out channelKey);
            if (channel == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR2.GetString(SR2.Error_CannotProvideChannel, logicalChannel.ConfigurationName, logicalChannel.CustomAddress)));
            }

            PooledChannelTicket pooledChannel = new PooledChannelTicket(this, channel, channelKey, workflowId, logicalChannel.InstanceId);
            return pooledChannel;
        }

        TimeSpan ConvertToTimeSpan(string value)
        {
            if (string.Equals(value, ChannelManagerService.InfiniteTimeSpanValue, StringComparison.OrdinalIgnoreCase))
            {
                return TimeSpan.MaxValue;
            }

            return TimeSpan.Parse(value, CultureInfo.InvariantCulture);
        }

        internal abstract class ChannelTicket : IDisposable
        {
            bool disposed;

            protected ChannelTicket()
            {
            }

            public abstract IChannel Channel
            {
                get;
            }

            public void Dispose()
            {
                if (!this.disposed)
                {
                    this.disposed = true;
                    this.Close();
                }
            }

            protected virtual void Close()
            {
            }
        }

        internal class PooledChannelTicket : ChannelTicket
        {
            ChannelManager.PooledChannel channel;
            ChannelPoolKey channelPoolKey;
            Guid logicalChannelId;

            ChannelManagerService service;
            Guid workflowId;

            public PooledChannelTicket(ChannelManagerService service, ChannelManager.PooledChannel channel, ChannelPoolKey channelPoolKey, Guid workflowId, Guid logicalChannelId)
                : base()
            {
                this.service = service;
                this.channel = channel;
                this.channelPoolKey = channelPoolKey;
                this.workflowId = workflowId;
                this.logicalChannelId = logicalChannelId;
            }

            public override IChannel Channel
            {
                get
                {
                    return this.channel.InnerChannel;
                }
            }

            internal ChannelPoolKey ChannelPoolKey
            {
                get
                {
                    return this.channelPoolKey;
                }
            }

            internal Guid LogicalChannelId
            {
                get
                {
                    return this.logicalChannelId;
                }
            }

            internal ChannelManager.PooledChannel PooledChannel
            {
                get
                {
                    return this.channel;
                }
            }

            internal Guid WorkflowId
            {
                get
                {
                    return this.workflowId;
                }
            }

            protected override void Close()
            {
                DiagnosticUtility.DebugAssert(this.channel != null, "channel has been closed already.");
                if (this.channel != null)
                {
                    this.service.ReturnChannel(this);
                    this.channel = null;
                }
            }
        }

        internal class TransientChannelTicket : ChannelTicket
        {
            IChannel channel;
            ChannelFactory factory;

            public TransientChannelTicket(IChannel channel, ChannelFactory factory)
                : base()
            {
                this.channel = channel;
                this.factory = factory;
            }

            public override IChannel Channel
            {
                get
                {
                    return this.channel;
                }
            }

            protected override void Close()
            {
                if (this.channel != null)
                {
                    ChannelManagerHelpers.CloseCommunicationObject(this.channel);
                    this.channel = null;
                }
                if (this.factory != null)
                {
                    ChannelManagerHelpers.CloseCommunicationObject(this.factory);
                    this.factory = null;
                }
            }
        }
    }
}
