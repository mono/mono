//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Description;
    using System.ServiceModel;

    public sealed partial class ServiceThrottlingElement : BehaviorExtensionElement
    {
        public ServiceThrottlingElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.MaxConcurrentCalls, DefaultValue = ServiceThrottle.DefaultMaxConcurrentCalls)]
        [IntegerValidator(MinValue = 1)]
        public int MaxConcurrentCalls
        {
            get { return (int)base[ConfigurationStrings.MaxConcurrentCalls]; }
            set { base[ConfigurationStrings.MaxConcurrentCalls] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxConcurrentSessions, DefaultValue = ServiceThrottle.DefaultMaxConcurrentSessions)]
        [IntegerValidator(MinValue = 1)]
        public int MaxConcurrentSessions
        {
            get { return (int)base[ConfigurationStrings.MaxConcurrentSessions]; }
            set { base[ConfigurationStrings.MaxConcurrentSessions] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxConcurrentInstances, DefaultValue = ServiceThrottle.DefaultMaxConcurrentCalls + ServiceThrottle.DefaultMaxConcurrentSessions)]
        [IntegerValidator(MinValue = 1)]
        public int MaxConcurrentInstances
        {
            get { return (int)base[ConfigurationStrings.MaxConcurrentInstances]; }
            set { base[ConfigurationStrings.MaxConcurrentInstances] = value; }
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);

            ServiceThrottlingElement source = (ServiceThrottlingElement)from;
#pragma warning suppress 56506 //Microsoft; base.CopyFrom() checks for 'from' being null
            this.MaxConcurrentCalls = source.MaxConcurrentCalls;
            this.MaxConcurrentSessions = source.MaxConcurrentSessions;
            this.MaxConcurrentInstances = source.MaxConcurrentInstances;
        }

        protected internal override object CreateBehavior()
        {
            ServiceThrottlingBehavior behavior = new ServiceThrottlingBehavior();

            PropertyInformationCollection propertyInfo = this.ElementInformation.Properties;
            if (propertyInfo[ConfigurationStrings.MaxConcurrentCalls].ValueOrigin != PropertyValueOrigin.Default)
            {
                behavior.MaxConcurrentCalls = this.MaxConcurrentCalls;
            }

            if (propertyInfo[ConfigurationStrings.MaxConcurrentSessions].ValueOrigin != PropertyValueOrigin.Default)
            {
                behavior.MaxConcurrentSessions = this.MaxConcurrentSessions;
            }

            if (propertyInfo[ConfigurationStrings.MaxConcurrentInstances].ValueOrigin != PropertyValueOrigin.Default)
            {
                behavior.MaxConcurrentInstances = this.MaxConcurrentInstances;
            }

            return behavior;
        }

        public override Type BehaviorType
        {
            get { return typeof(ServiceThrottlingBehavior); }
        }

    }
}



