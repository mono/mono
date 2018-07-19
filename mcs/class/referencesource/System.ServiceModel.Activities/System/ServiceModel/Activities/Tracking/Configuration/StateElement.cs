//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System.Configuration;
    using System.Runtime;
    using System.Diagnostics.CodeAnalysis;

    [Fx.Tag.XamlVisible(false)]
    public class StateElement : TrackingConfigurationElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty(TrackingConfigurationStrings.Name, typeof(System.String), "*", null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsKey));
                    this.properties = properties;
                }
                return this.properties;
            }
        }

        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationPropertyAttributeRule,
            Justification = "This property is defined by the base class to compute unique key.")]
        public override object ElementKey
        {
            get { return this.Name; }
        }

        [ConfigurationProperty(TrackingConfigurationStrings.Name, IsKey = true,
            DefaultValue = TrackingConfigurationStrings.StarWildcard)]
        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule,
            MessageId = "System.ServiceModel.Activities.Tracking.Configuration.StateElement.Name",
            Justification = "StringValidator verifies minimum size")]
        [StringValidator(MinLength = 0)]
        public string Name
        {
            get { return (string)base[TrackingConfigurationStrings.Name]; }
            set { base[TrackingConfigurationStrings.Name] = value; }
        }
    }
}
