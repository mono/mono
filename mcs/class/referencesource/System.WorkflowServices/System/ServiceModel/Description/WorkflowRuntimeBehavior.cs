//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Description
{
    using System.Globalization;
    using System.ServiceModel.Administration;
    using System.ServiceModel.Dispatcher;
    using System.Workflow.Runtime;
    using System.Workflow.Runtime.Configuration;
    using System.Workflow.Runtime.Hosting;

    [Obsolete("The WF3 types are deprecated.  Instead, please use the new WF4 types from System.Activities.*")]
    public class WorkflowRuntimeBehavior : IServiceBehavior, IWmiInstanceProvider
    {
        internal static readonly TimeSpan DefaultCachedInstanceExpiration = TimeSpan.Parse(DefaultCachedInstanceExpirationString, CultureInfo.InvariantCulture);
        //default of 10 minutes chosen to be in-parity with SM inactivity timeout for session.
        internal const string DefaultCachedInstanceExpirationString = "00:10:00";
        internal const string defaultName = "WorkflowRuntime";
        internal const bool DefaultValidateOnCreate = true;

        static WorkflowRuntimeSection defaultWorkflowRuntimeSection;

        TimeSpan cachedInstanceExpiration;
        bool validateOnCreate;
        WorkflowRuntime workflowRuntime = null;

        public WorkflowRuntimeBehavior() : this(null, DefaultCachedInstanceExpiration, DefaultValidateOnCreate)
        {
            // empty
        }

        internal WorkflowRuntimeBehavior(WorkflowRuntime workflowRuntime, TimeSpan cachedInstanceExpiration, bool validateOnCreate)
        {
            this.workflowRuntime = workflowRuntime;
            this.cachedInstanceExpiration = cachedInstanceExpiration;
            this.validateOnCreate = validateOnCreate;
        }

        public TimeSpan CachedInstanceExpiration
        {
            get
            {
                return this.cachedInstanceExpiration;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.cachedInstanceExpiration = value;
            }
        }

        public WorkflowRuntime WorkflowRuntime
        {
            get
            {
                if (this.workflowRuntime == null)
                {
                    this.workflowRuntime = new WorkflowRuntime(WorkflowRuntimeBehavior.DefaultWorkflowRuntimeSection);
                }
                return this.workflowRuntime;
            }
        }

        internal bool ValidateOnCreate
        {
            get { return this.validateOnCreate; }
        }

        static WorkflowRuntimeSection DefaultWorkflowRuntimeSection
        {
            get
            {
                if (defaultWorkflowRuntimeSection == null)
                {
                    WorkflowRuntimeSection tempSection = new WorkflowRuntimeSection();
                    tempSection.ValidateOnCreate = false;
                    tempSection.EnablePerformanceCounters = true;
                    tempSection.Name = defaultName;
                    defaultWorkflowRuntimeSection = tempSection;
                }
                return defaultWorkflowRuntimeSection;
            }
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {

        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            if (serviceHostBase == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceHostBase");
            }
            if (serviceHostBase.Extensions == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceHostBase.Extensions");
            }

            WorkflowInstanceLifetimeManagerExtension cachedInstanceExpirationExtension = new WorkflowInstanceLifetimeManagerExtension(
                this.WorkflowRuntime,
                this.CachedInstanceExpiration,
                this.WorkflowRuntime.GetService<WorkflowPersistenceService>() != null);
            serviceHostBase.Extensions.Add(cachedInstanceExpirationExtension);
        }

        void IWmiInstanceProvider.FillInstance(IWmiInstance wmiInstance)
        {
            wmiInstance.SetProperty("CachedInstanceExpiration", this.CachedInstanceExpiration.ToString());
        }

        string IWmiInstanceProvider.GetInstanceType()
        {
            return "WorkflowRuntimeBehavior";
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            ValidateWorkflowRuntime(this.WorkflowRuntime);
        }

        void ValidateWorkflowRuntime(WorkflowRuntime workflowRuntime)
        {
            if (workflowRuntime.IsStarted)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.WorkflowRuntimeStartedBeforeHostOpen)));
            }

            WorkflowSchedulerService workflowSchedulerService = workflowRuntime.GetService<WorkflowSchedulerService>();

            if (!(workflowSchedulerService is SynchronizationContextWorkflowSchedulerService))
            {
                if (workflowSchedulerService != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.WrongSchedulerServiceRegistered)));
                }
                workflowRuntime.AddService(new SynchronizationContextWorkflowSchedulerService());
            }
        }

    }
}
