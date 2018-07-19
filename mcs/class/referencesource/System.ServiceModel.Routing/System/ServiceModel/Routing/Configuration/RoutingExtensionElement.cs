//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Routing.Configuration
{
    using System;
    using System.Linq;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;

    public sealed class RoutingExtensionElement : BehaviorExtensionElement
    {
        public RoutingExtensionElement()
        {
            this.RouteOnHeadersOnly = RoutingConfiguration.DefaultRouteOnHeadersOnly;
        }

        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationPropertyAttributeRule, Justification = "this is not a configuration property")]
        public override Type BehaviorType
        {
            get { return typeof(RoutingBehavior); }
        }

        [ConfigurationProperty(ConfigurationStrings.RouteOnHeadersOnly, DefaultValue = RoutingConfiguration.DefaultRouteOnHeadersOnly, Options = ConfigurationPropertyOptions.None)]
        public bool RouteOnHeadersOnly
        {
            get
            {
                return (bool)this[ConfigurationStrings.RouteOnHeadersOnly];
            }
            set
            {
                this[ConfigurationStrings.RouteOnHeadersOnly] = value;
            }
        }

        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule, Justification = "fxcop didn't like [StringValidator(MinLength = 0)]")]
        [ConfigurationProperty(ConfigurationStrings.FilterTableName, DefaultValue = null)]
        public string FilterTableName
        {
            get
            {
                return (string)this[ConfigurationStrings.FilterTableName];
            }
            set
            {
                this[ConfigurationStrings.FilterTableName] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.SoapProcessingEnabled, DefaultValue = RoutingConfiguration.DefaultSoapProcessingEnabled)]
        public bool SoapProcessingEnabled
        {
            get
            {
                return (bool)this[ConfigurationStrings.SoapProcessingEnabled];
            }
            set
            {
                this[ConfigurationStrings.SoapProcessingEnabled] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.EnsureOrderedDispatch, DefaultValue = RoutingConfiguration.DefaultEnsureOrderedDispatch)]
        public bool EnsureOrderedDispatch
        {
            get
            {
                return (bool)this[ConfigurationStrings.EnsureOrderedDispatch];
            }
            set
            {
                this[ConfigurationStrings.EnsureOrderedDispatch] = value;
            }
        }

        protected internal override object CreateBehavior()
        {
            RoutingConfiguration config;
            if (string.IsNullOrEmpty(this.FilterTableName))
            {
                config = new RoutingConfiguration();
                config.RouteOnHeadersOnly = this.RouteOnHeadersOnly;
            }
            else
            {
                config = new RoutingConfiguration(RoutingSection.CreateFilterTable(this.FilterTableName), this.RouteOnHeadersOnly);
            }

            config.SoapProcessingEnabled = this.SoapProcessingEnabled;
            config.EnsureOrderedDispatch = this.EnsureOrderedDispatch;
            RoutingBehavior behavior = new RoutingBehavior(config);
            //behavior.Impersonation = this.Impersonation;
            return behavior;
        }
    }
}
