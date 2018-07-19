//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System.Configuration;
    using System.Runtime;
    using System.Diagnostics.CodeAnalysis;
    using System.ServiceModel.Configuration;
    using System.Activities.Tracking;
    using System.ServiceModel.Activities.Configuration;

    [Fx.Tag.XamlVisible(false)]
    public class ProfileElement : TrackingConfigurationElement
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
                    properties.Add(new ConfigurationProperty(TrackingConfigurationStrings.ImplementationVisibility, typeof(System.Activities.Tracking.ImplementationVisibility), System.Activities.Tracking.ImplementationVisibility.RootScope, null, new System.ServiceModel.Activities.Configuration.ServiceModelActivitiesEnumValidator(typeof(System.ServiceModel.Activities.Tracking.Configuration.ImplementationVisibilityHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("", typeof(System.ServiceModel.Activities.Tracking.Configuration.ProfileWorkflowElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.IsDefaultCollection));
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
            MessageId = "System.ServiceModel.Activities.Tracking.Configuration.ProfileElement.Name",
            Justification = "StringValidator verifies minimum size")]
        public string Name
        {
            get { return (string)base[TrackingConfigurationStrings.Name]; }
            set { base[TrackingConfigurationStrings.Name] = value; }
        }

        [ConfigurationProperty(TrackingConfigurationStrings.ImplementationVisibility, DefaultValue = ImplementationVisibility.RootScope)]
        [ServiceModelActivitiesEnumValidator(typeof(ImplementationVisibilityHelper))]
        public ImplementationVisibility ImplementationVisibility
        {
            get { return (ImplementationVisibility)base[TrackingConfigurationStrings.ImplementationVisibility]; }
            set { base[TrackingConfigurationStrings.ImplementationVisibility] = value; }
        }

        [ConfigurationProperty("", IsDefaultCollection = true)]
        public ProfileWorkflowElementCollection Workflows
        {
            get
            {
                return (ProfileWorkflowElementCollection)base[""];
            }
        }
    }
}
