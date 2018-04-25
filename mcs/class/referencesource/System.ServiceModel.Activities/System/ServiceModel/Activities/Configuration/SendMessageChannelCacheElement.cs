//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Activities.Configuration
{
    using System.ComponentModel;
    using System.Configuration;
    using System.Runtime;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Activities.Description;
    using System.ServiceModel.Configuration;

    public sealed class SendMessageChannelCacheElement : BehaviorExtensionElement
    {
        ConfigurationPropertyCollection properties;
        
        public SendMessageChannelCacheElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.AllowUnsafeCaching, DefaultValue = ChannelCacheDefaults.DefaultAllowUnsafeSharing)]
        public bool AllowUnsafeCaching
        {
            get { return (bool)base[ConfigurationStrings.AllowUnsafeCaching]; }
            set { base[ConfigurationStrings.AllowUnsafeCaching] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.FactorySettings)]
        public FactorySettingsElement FactorySettings
        {
            get { return (FactorySettingsElement)base[ConfigurationStrings.FactorySettings]; }
        }

        [ConfigurationProperty(ConfigurationStrings.ChannelSettings)]
        public ChannelSettingsElement ChannelSettings
        {
            get { return (ChannelSettingsElement)base[ConfigurationStrings.ChannelSettings]; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Configuration", "Configuration102:ConfigurationPropertyAttributeRule", MessageId = "System.ServiceModel.Activities.Configuration.SendMessageChannelCacheElement.BehaviorType", Justification = "Not a configurable property; a property that had to be overridden from abstract parent class")]
        public override Type BehaviorType
        {
            get { return typeof(SendMessageChannelCacheBehavior); }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty(ConfigurationStrings.AllowUnsafeCaching, typeof(bool), ChannelCacheDefaults.DefaultAllowUnsafeSharing));
                    properties.Add(new ConfigurationProperty(ConfigurationStrings.FactorySettings, typeof(FactorySettingsElement)));
                    properties.Add(new ConfigurationProperty(ConfigurationStrings.ChannelSettings, typeof(ChannelSettingsElement)));
                    this.properties = properties;
                }
                return this.properties;
            }
        }

        protected internal override object CreateBehavior()
        {
            return new SendMessageChannelCacheBehavior()
            {
                AllowUnsafeCaching = this.AllowUnsafeCaching,
                FactorySettings = new ChannelCacheSettings { IdleTimeout = FactorySettings.IdleTimeout, LeaseTimeout = FactorySettings.LeaseTimeout, MaxItemsInCache = FactorySettings.MaxItemsInCache },
                ChannelSettings = new ChannelCacheSettings { IdleTimeout = ChannelSettings.IdleTimeout, LeaseTimeout = ChannelSettings.LeaseTimeout, MaxItemsInCache = ChannelSettings.MaxItemsInCache }
            };
        }


    }
}




