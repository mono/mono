//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System.Configuration;
    using System.Activities.Tracking;
    using System.Runtime;
    using System.Diagnostics.CodeAnalysis;

    [Fx.Tag.XamlVisible(false)]
    public class VariableElement : TrackingConfigurationElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty(TrackingConfigurationStrings.Name, typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsRequired | System.Configuration.ConfigurationPropertyOptions.IsKey));
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

        [ConfigurationProperty(TrackingConfigurationStrings.Name, IsKey = true, IsRequired = true)]
        [StringValidator(MinLength = 0)]
        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule,
            MessageId = "System.ServiceModel.Activities.Tracking.Configuration.VariableElement.Name",
            Justification = "StringValidator verifies minimum size")]
        public string Name
        {
            get { return (string)base[TrackingConfigurationStrings.Name]; }
            set { base[TrackingConfigurationStrings.Name] = value; }
        }
    }
}
