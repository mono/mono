//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Activities.Configuration
{
    using System.Configuration;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Activities.Description;

    public sealed class WorkflowUnhandledExceptionElement : BehaviorExtensionElement
    {
        ConfigurationPropertyCollection properties;
        const string action = "action";

        public WorkflowUnhandledExceptionElement()
        {
        }

        [ConfigurationProperty(action, DefaultValue = WorkflowUnhandledExceptionBehavior.defaultAction)]
        [ServiceModelActivitiesEnumValidator(typeof(WorkflowUnhandledExceptionActionHelper))]
        public WorkflowUnhandledExceptionAction Action
        {
            get { return (WorkflowUnhandledExceptionAction)base[action]; }
            set { base[action] = value; }
        }

        protected internal override object CreateBehavior()
        {
            return new WorkflowUnhandledExceptionBehavior() { Action = this.Action };
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Configuration", "Configuration102:ConfigurationPropertyAttributeRule", MessageId = "System.ServiceModel.Activities.Configuration.WorkflowUnhandledExceptionElement.BehaviorType", Justification = "Not a configurable property; a property that had to be overridden from abstract parent class")]
        public override Type BehaviorType
        {
            get { return typeof(WorkflowUnhandledExceptionBehavior); }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty(action, typeof(WorkflowUnhandledExceptionAction), WorkflowUnhandledExceptionBehavior.defaultAction, null, new ServiceModelActivitiesEnumValidator(typeof(WorkflowUnhandledExceptionActionHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }

    }
}




