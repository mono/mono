//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Activities.Description
{
    using System.Activities.DurableInstancing;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Xml.Linq;
    using System.Globalization;

    [Fx.Tag.XamlVisible(false)]
    public class SqlWorkflowInstanceStoreBehavior : IServiceBehavior
    {        
        internal const int defaultMaximumRetries = 4;
        internal const string defaultHostRenewalString = "00:00:30.0";
        static TimeSpan defaultHostRenewalPeriod = TimeSpan.Parse(defaultHostRenewalString, CultureInfo.InvariantCulture);
        internal const string defaultRunnableInstancesDetectionPeriodString = "00:00:05.0";
        static TimeSpan defaultRunnableInstancesDetectionPeriod = TimeSpan.Parse(defaultRunnableInstancesDetectionPeriodString, CultureInfo.InvariantCulture);

        internal const InstanceEncodingOption defaultEncodingOption = InstanceEncodingOption.GZip;
        internal const InstanceCompletionAction defaultInstanceCompletionAction = InstanceCompletionAction.DeleteAll;
        internal const InstanceLockedExceptionAction defaultInstanceLockedExceptionAction = InstanceLockedExceptionAction.NoRetry;
     

        public SqlWorkflowInstanceStoreBehavior() :
            this(null)
        {
        }

        public SqlWorkflowInstanceStoreBehavior(string connectionString)
        {
            this.SqlWorkflowInstanceStore = new SqlWorkflowInstanceStore(connectionString)
            {
                InstanceEncodingOption = defaultEncodingOption,
                InstanceCompletionAction = defaultInstanceCompletionAction,
                InstanceLockedExceptionAction = defaultInstanceLockedExceptionAction,
                HostLockRenewalPeriod = defaultHostRenewalPeriod,
                RunnableInstancesDetectionPeriod = defaultRunnableInstancesDetectionPeriod, 
                EnqueueRunCommands = true
            };
        }

        public InstanceEncodingOption InstanceEncodingOption
        {
            get
            {
                return this.SqlWorkflowInstanceStore.InstanceEncodingOption;
            }
            set
            {
                this.SqlWorkflowInstanceStore.InstanceEncodingOption = value;
            }
        }

        public InstanceCompletionAction InstanceCompletionAction
        {
            get
            {
                return this.SqlWorkflowInstanceStore.InstanceCompletionAction;
            }
            set
            {
                this.SqlWorkflowInstanceStore.InstanceCompletionAction = value;
            }
        }

        public InstanceLockedExceptionAction InstanceLockedExceptionAction
        {
            get
            {
                return this.SqlWorkflowInstanceStore.InstanceLockedExceptionAction;
            }
            set
            {
                this.SqlWorkflowInstanceStore.InstanceLockedExceptionAction = value;
            }
        }

        public string ConnectionString
        {
            get
            {
                return this.SqlWorkflowInstanceStore.ConnectionString;
            }
            set
            {
                this.SqlWorkflowInstanceStore.ConnectionString = value;
            }
        }

        public TimeSpan HostLockRenewalPeriod
        {
            get
            {
                return this.SqlWorkflowInstanceStore.HostLockRenewalPeriod;
            }
            set
            {
                TimeoutHelper.ThrowIfNonPositiveArgument(value);
                this.SqlWorkflowInstanceStore.HostLockRenewalPeriod = value;
            }
        }

        public TimeSpan RunnableInstancesDetectionPeriod
        {
            get
            {
                return this.SqlWorkflowInstanceStore.RunnableInstancesDetectionPeriod;
            }
            set
            {
                TimeoutHelper.ThrowIfNonPositiveArgument(value);
                this.SqlWorkflowInstanceStore.RunnableInstancesDetectionPeriod = value;
            }
        }

        public int MaxConnectionRetries
        {
            get
            {
                if (this.SqlWorkflowInstanceStore != null)
                {
                    return this.SqlWorkflowInstanceStore.MaxConnectionRetries;
                }
                else
                {
                    return defaultMaximumRetries;
                }
            }
            set
            {
                Fx.Assert(this.SqlWorkflowInstanceStore != null, "The SqlWorkflowInstanceStore should never be null");
                if (this.SqlWorkflowInstanceStore != null)
                {
                    this.SqlWorkflowInstanceStore.MaxConnectionRetries = value;
                }
            }
        }

        SqlWorkflowInstanceStore SqlWorkflowInstanceStore
        {
            get;
            set;
        }

        public void Promote(string name, IEnumerable<XName> promoteAsSqlVariant, IEnumerable<XName> promoteAsBinary)
        {
            this.SqlWorkflowInstanceStore.Promote(name, promoteAsSqlVariant, promoteAsBinary);
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            if (serviceHostBase == null)
            {
                throw FxTrace.Exception.ArgumentNull("serviceHostBase");
            }

            WorkflowServiceHost workflowServiceHost = serviceHostBase as WorkflowServiceHost;

            if (workflowServiceHost != null)
            {
                workflowServiceHost.DurableInstancingOptions.InstanceStore = this.SqlWorkflowInstanceStore;
            }
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
         
        }
    }
}
