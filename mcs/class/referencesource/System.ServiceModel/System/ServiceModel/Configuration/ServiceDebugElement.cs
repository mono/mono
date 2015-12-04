//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel.Description;
    using System.Collections.Generic;

    public sealed partial class ServiceDebugElement : BehaviorExtensionElement
    {
        public ServiceDebugElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.HttpHelpPageEnabled, DefaultValue = true)]
        public bool HttpHelpPageEnabled
        {
            get { return (bool)base[ConfigurationStrings.HttpHelpPageEnabled]; }
            set { base[ConfigurationStrings.HttpHelpPageEnabled] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.HttpHelpPageUrl)]
        public Uri HttpHelpPageUrl
        {
            get { return (Uri)base[ConfigurationStrings.HttpHelpPageUrl]; }
            set { base[ConfigurationStrings.HttpHelpPageUrl] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.HttpsHelpPageEnabled, DefaultValue = true)]
        public bool HttpsHelpPageEnabled
        {
            get { return (bool)base[ConfigurationStrings.HttpsHelpPageEnabled]; }
            set { base[ConfigurationStrings.HttpsHelpPageEnabled] = value; }
        }
        
        [ConfigurationProperty(ConfigurationStrings.HttpsHelpPageUrl)]
        public Uri HttpsHelpPageUrl
        {
            get { return (Uri)base[ConfigurationStrings.HttpsHelpPageUrl]; }
            set { base[ConfigurationStrings.HttpsHelpPageUrl] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.HttpHelpPageBinding, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string HttpHelpPageBinding
        {
            get { return (string)base[ConfigurationStrings.HttpHelpPageBinding]; }
            set { base[ConfigurationStrings.HttpHelpPageBinding] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.HttpHelpPageBindingConfiguration, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string HttpHelpPageBindingConfiguration
        {
            get { return (string)base[ConfigurationStrings.HttpHelpPageBindingConfiguration]; }
            set { base[ConfigurationStrings.HttpHelpPageBindingConfiguration] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.HttpsHelpPageBinding, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string HttpsHelpPageBinding
        {
            get { return (string)base[ConfigurationStrings.HttpsHelpPageBinding]; }
            set { base[ConfigurationStrings.HttpsHelpPageBinding] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.HttpsHelpPageBindingConfiguration, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string HttpsHelpPageBindingConfiguration
        {
            get { return (string)base[ConfigurationStrings.HttpsHelpPageBindingConfiguration]; }
            set { base[ConfigurationStrings.HttpsHelpPageBindingConfiguration] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.IncludeExceptionDetailInFaults, DefaultValue = false)]
        public bool IncludeExceptionDetailInFaults
        {
            get { return (bool)base[ConfigurationStrings.IncludeExceptionDetailInFaults]; }
            set { base[ConfigurationStrings.IncludeExceptionDetailInFaults] = value; }
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);

            ServiceDebugElement source = (ServiceDebugElement)from;
#pragma warning suppress 56506 //[....]; base.CopyFrom() check for 'from' being null
            this.HttpHelpPageEnabled = source.HttpHelpPageEnabled;
            this.HttpHelpPageUrl = source.HttpHelpPageUrl;
            this.HttpsHelpPageEnabled = source.HttpsHelpPageEnabled;
            this.HttpsHelpPageUrl = source.HttpsHelpPageUrl;
            this.IncludeExceptionDetailInFaults = source.IncludeExceptionDetailInFaults;
            this.HttpHelpPageBinding = source.HttpHelpPageBinding;
            this.HttpHelpPageBindingConfiguration = source.HttpHelpPageBindingConfiguration;
            this.HttpsHelpPageBinding = source.HttpsHelpPageBinding;
            this.HttpsHelpPageBindingConfiguration = source.HttpsHelpPageBindingConfiguration;

        }

        protected internal override object CreateBehavior()
        {
            ServiceDebugBehavior behavior = new ServiceDebugBehavior();

            behavior.HttpHelpPageEnabled = this.HttpHelpPageEnabled;
            behavior.HttpHelpPageUrl = this.HttpHelpPageUrl;
            behavior.HttpsHelpPageEnabled = this.HttpsHelpPageEnabled;
            behavior.HttpsHelpPageUrl = this.HttpsHelpPageUrl;
            behavior.IncludeExceptionDetailInFaults = this.IncludeExceptionDetailInFaults;
            if (!String.IsNullOrEmpty(this.HttpHelpPageBinding))
                behavior.HttpHelpPageBinding = ConfigLoader.LookupBinding(this.HttpHelpPageBinding, this.HttpHelpPageBindingConfiguration);
            if (!String.IsNullOrEmpty(this.HttpsHelpPageBinding))
                behavior.HttpsHelpPageBinding = ConfigLoader.LookupBinding(this.HttpsHelpPageBinding, this.HttpsHelpPageBindingConfiguration);

            return behavior;
        }

        public override Type BehaviorType
        {
            get { return typeof(ServiceDebugBehavior); }
        }
    }
}




