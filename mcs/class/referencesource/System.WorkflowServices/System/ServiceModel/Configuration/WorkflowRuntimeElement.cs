//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Configuration
{
    using System.ComponentModel;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel.Description;
    using System.Workflow.Runtime;
    using System.Workflow.Runtime.Configuration;

    [Obsolete("The WF3 types are deprecated.  Instead, please use the new WF4 types from System.Activities.*")]
    public class WorkflowRuntimeElement : BehaviorExtensionElement
    {
        const string cachedInstanceExpiration = "cachedInstanceExpiration";
        const string commonParameters = "commonParameters";
        const string enablePerfCounters = "enablePerformanceCounters";
        const string name = "name";
        const string services = "services";
        const string validateOnCreate = "validateOnCreate";

        ConfigurationPropertyCollection configProperties = null;

        WorkflowRuntimeSection wrtSection = null;


        public WorkflowRuntimeElement()
        {

        }

        // This property is not supposed to be exposed in config. 
        [SuppressMessage("Configuration", "Configuration102:ConfigurationPropertyAttributeRule", MessageId = "System.ServiceModel.Configuration.WorkflowRuntimeElement.BehaviorType")]
        public override Type BehaviorType
        {
            get
            {
                return typeof(WorkflowRuntimeBehavior);
            }
        }

        [ConfigurationProperty(cachedInstanceExpiration, IsRequired = false, DefaultValue = WorkflowRuntimeBehavior.DefaultCachedInstanceExpirationString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [PositiveTimeSpanValidator]
        public TimeSpan CachedInstanceExpiration
        {
            get
            {
                return (TimeSpan) base[cachedInstanceExpiration];
            }
            set
            {
                base[cachedInstanceExpiration] = value;
            }
        }

        [ConfigurationProperty(commonParameters, DefaultValue = null)]
        public NameValueConfigurationCollection CommonParameters
        {
            get
            {
                return (NameValueConfigurationCollection) base[commonParameters];
            }
        }

        [ConfigurationProperty(enablePerfCounters, DefaultValue = true)]
        public bool EnablePerformanceCounters
        {
            get
            {
                return (bool) base[enablePerfCounters];
            }
            set
            {
                base[enablePerfCounters] = value;
            }
        }


        [ConfigurationProperty(name, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string Name
        {
            get
            {
                return (string) base[name];
            }
            set
            {
                base[name] = value;
            }
        }

        [ConfigurationProperty(services, DefaultValue = null)]
        public ExtendedWorkflowRuntimeServiceElementCollection Services
        {
            get
            {
                return (ExtendedWorkflowRuntimeServiceElementCollection) base[services];
            }
        }

        [ConfigurationProperty(validateOnCreate, DefaultValue = WorkflowRuntimeBehavior.DefaultValidateOnCreate)]
        public bool ValidateOnCreate
        {
            get
            {
                return (bool) base[validateOnCreate];
            }
            set
            {
                base[validateOnCreate] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.configProperties == null)
                {
                    this.configProperties = new ConfigurationPropertyCollection();
                    configProperties.Add(new ConfigurationProperty(name, typeof(string), null));
                    configProperties.Add(new ConfigurationProperty(validateOnCreate, typeof(bool), true));
                    configProperties.Add(new ConfigurationProperty(enablePerfCounters, typeof(bool), true));
                    configProperties.Add(new ConfigurationProperty(services, typeof(ExtendedWorkflowRuntimeServiceElementCollection), null));
                    configProperties.Add(new ConfigurationProperty(commonParameters, typeof(NameValueConfigurationCollection), null));
                    configProperties.Add(new ConfigurationProperty(cachedInstanceExpiration, typeof(TimeSpan), WorkflowRuntimeBehavior.DefaultCachedInstanceExpiration));
                }

                return this.configProperties;
            }
        }

        protected internal override object CreateBehavior()
        {
            return new WorkflowRuntimeBehavior(new WorkflowRuntime(CreateWorkflowRuntimeSection()), this.CachedInstanceExpiration, this.ValidateOnCreate);
        }

        WorkflowRuntimeSection CreateWorkflowRuntimeSection()
        {
            if (wrtSection == null)
            {
                wrtSection = new WorkflowRuntimeSection();
                wrtSection.Name = this.Name;
                wrtSection.ValidateOnCreate = false;
                wrtSection.EnablePerformanceCounters = this.EnablePerformanceCounters;

                foreach (WorkflowRuntimeServiceElement wrtSvcElement in this.Services)
                {
                    wrtSection.Services.Add(wrtSvcElement);
                }

                foreach (NameValueConfigurationElement nameValueElement in this.CommonParameters)
                {
                    wrtSection.CommonParameters.Add(nameValueElement);
                }
            }
            return wrtSection;
        }
    }
}
