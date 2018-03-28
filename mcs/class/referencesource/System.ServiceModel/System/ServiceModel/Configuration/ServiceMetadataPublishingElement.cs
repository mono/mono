//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel.Description;
    using System.Collections.Generic;
    using System.ComponentModel;

    public sealed partial class ServiceMetadataPublishingElement : BehaviorExtensionElement
    {
        public ServiceMetadataPublishingElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.ExternalMetadataLocation)]
        public Uri ExternalMetadataLocation
        {
            get { return (Uri)base[ConfigurationStrings.ExternalMetadataLocation]; }
            set { base[ConfigurationStrings.ExternalMetadataLocation] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.HttpGetEnabled, DefaultValue = false)]
        public bool HttpGetEnabled
        {
            get { return (bool)base[ConfigurationStrings.HttpGetEnabled]; }
            set { base[ConfigurationStrings.HttpGetEnabled] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.HttpGetUrl)]
        public Uri HttpGetUrl
        {
            get { return (Uri)base[ConfigurationStrings.HttpGetUrl]; }
            set { base[ConfigurationStrings.HttpGetUrl] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.HttpsGetEnabled, DefaultValue = false)]
        public bool HttpsGetEnabled
        {
            get { return (bool)base[ConfigurationStrings.HttpsGetEnabled]; }
            set { base[ConfigurationStrings.HttpsGetEnabled] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.HttpsGetUrl)]
        public Uri HttpsGetUrl
        {
            get { return (Uri)base[ConfigurationStrings.HttpsGetUrl]; }
            set { base[ConfigurationStrings.HttpsGetUrl] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.HttpGetBinding, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string HttpGetBinding
        {
            get { return (string)base[ConfigurationStrings.HttpGetBinding]; }
            set { base[ConfigurationStrings.HttpGetBinding] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.HttpGetBindingConfiguration, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string HttpGetBindingConfiguration
        {
            get { return (string)base[ConfigurationStrings.HttpGetBindingConfiguration]; }
            set { base[ConfigurationStrings.HttpGetBindingConfiguration] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.HttpsGetBinding, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string HttpsGetBinding
        {
            get { return (string)base[ConfigurationStrings.HttpsGetBinding]; }
            set { base[ConfigurationStrings.HttpsGetBinding] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.HttpsGetBindingConfiguration, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string HttpsGetBindingConfiguration
        {
            get { return (string)base[ConfigurationStrings.HttpsGetBindingConfiguration]; }
            set { base[ConfigurationStrings.HttpsGetBindingConfiguration] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.PolicyVersion, DefaultValue = ConfigurationStrings.Default)]
        [TypeConverter(typeof(PolicyVersionConverter))]
        public PolicyVersion PolicyVersion
        {
            get { return (PolicyVersion)base[ConfigurationStrings.PolicyVersion]; }
            set { base[ConfigurationStrings.PolicyVersion] = value; }
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);

            ServiceMetadataPublishingElement source = (ServiceMetadataPublishingElement)from;
#pragma warning suppress 56506 //Microsoft; base.CopyFrom() check for 'from' being null
            this.HttpGetEnabled = source.HttpGetEnabled;
            this.HttpGetUrl = source.HttpGetUrl;
            this.HttpsGetEnabled = source.HttpsGetEnabled;
            this.HttpsGetUrl = source.HttpsGetUrl;
            this.ExternalMetadataLocation = source.ExternalMetadataLocation;
            this.PolicyVersion = source.PolicyVersion;
            this.HttpGetBinding = source.HttpGetBinding;
            this.HttpGetBindingConfiguration = source.HttpGetBindingConfiguration;
            this.HttpsGetBinding = source.HttpsGetBinding;
            this.HttpsGetBindingConfiguration = source.HttpsGetBindingConfiguration;

        }

        protected internal override object CreateBehavior()
        {
            ServiceMetadataBehavior behavior = new ServiceMetadataBehavior();

            behavior.HttpGetEnabled = this.HttpGetEnabled;
            behavior.HttpGetUrl = this.HttpGetUrl;
            behavior.HttpsGetEnabled = this.HttpsGetEnabled;
            behavior.HttpsGetUrl = this.HttpsGetUrl;
            behavior.ExternalMetadataLocation = this.ExternalMetadataLocation;
            behavior.MetadataExporter.PolicyVersion = this.PolicyVersion;
            if (!String.IsNullOrEmpty(this.HttpGetBinding))
                behavior.HttpGetBinding = ConfigLoader.LookupBinding(this.HttpGetBinding, this.HttpGetBindingConfiguration);
            if (!String.IsNullOrEmpty(this.HttpsGetBinding))
                behavior.HttpsGetBinding = ConfigLoader.LookupBinding(this.HttpsGetBinding, this.HttpsGetBindingConfiguration);

            return behavior;
        }

        public override Type BehaviorType
        {
            get { return typeof(ServiceMetadataBehavior); }
        }
    }
}



