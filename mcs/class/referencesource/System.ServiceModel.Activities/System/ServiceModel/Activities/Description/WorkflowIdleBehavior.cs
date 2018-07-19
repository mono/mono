//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Description
{
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    [Fx.Tag.XamlVisible(false)]
    public class WorkflowIdleBehavior : IServiceBehavior
    {
        internal const string defaultTimeToPersistString = "Infinite";
        internal static TimeSpan defaultTimeToPersist = TimeSpan.MaxValue;
        internal const string defaultTimeToUnloadString = "00:01:00";
        internal static TimeSpan defaultTimeToUnload = TimeSpan.Parse(defaultTimeToUnloadString, CultureInfo.InvariantCulture);

        TimeSpan timeToPersist;
        TimeSpan timeToUnload;

        public WorkflowIdleBehavior()
        {
            this.timeToPersist = defaultTimeToPersist;
            this.timeToUnload = defaultTimeToUnload;
        }

        public TimeSpan TimeToPersist
        {
            get { return this.timeToPersist; }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw FxTrace.Exception.ArgumentOutOfRange("value", value, SR.ErrorTimeToPersistLessThanZero);
                }
                this.timeToPersist = value;
            }
        }

        public TimeSpan TimeToUnload
        {
            get { return this.timeToUnload; }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw FxTrace.Exception.ArgumentOutOfRange("value", value, SR.ErrorTimeToUnloadLessThanZero);
                }
                this.timeToUnload = value;
            }
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            WorkflowServiceHost workflowServiceHost = serviceHostBase as WorkflowServiceHost;
            if (workflowServiceHost != null)
            {
                workflowServiceHost.IdleTimeToPersist = this.TimeToPersist;
                workflowServiceHost.IdleTimeToUnload = this.TimeToUnload;
            }
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            if (serviceDescription == null)
            {
                throw FxTrace.Exception.ArgumentNull("serviceDescription");
            }

            if (serviceHostBase == null)
            {
                throw FxTrace.Exception.ArgumentNull("serviceHostBase");
            }
        }
    }
}
