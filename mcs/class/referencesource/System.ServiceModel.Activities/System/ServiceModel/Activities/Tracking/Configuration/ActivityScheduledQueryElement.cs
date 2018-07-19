//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System.Configuration;
    using System.Runtime;
    using System.Activities.Tracking;
    using System.Diagnostics.CodeAnalysis;

    [Fx.Tag.XamlVisible(false)]
    public class ActivityScheduledQueryElement : TrackingQueryElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty(TrackingConfigurationStrings.ActivityName, typeof(System.String), "*", null, new System.Configuration.StringValidator(1, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsKey));
                    properties.Add(new ConfigurationProperty(TrackingConfigurationStrings.ChildActivityName, typeof(System.String), "*", null, new System.Configuration.StringValidator(1, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsKey));
                    this.properties = properties;
                }
                return this.properties;
            }
        }

        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule,
            MessageId = "System.ServiceModel.Activities.Tracking.Configuration.ActivityScheduledQueryElement.ChildActivityName",
            Justification = "StringValidator verifies minimum size")]
        [ConfigurationProperty(TrackingConfigurationStrings.ActivityName, IsKey = true,
            DefaultValue = TrackingConfigurationStrings.StarWildcard)]
        [StringValidator(MinLength = 1)]
        public string ActivityName
        {
            get { return (string)base[TrackingConfigurationStrings.ActivityName]; }
            set { base[TrackingConfigurationStrings.ActivityName] = value; }
        }

        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule,
            MessageId = "System.ServiceModel.Activities.Tracking.Configuration.ActivityScheduledQueryElement.ChildActivityName",
            Justification = "StringValidator verifies minimum size")]
        [ConfigurationProperty(TrackingConfigurationStrings.ChildActivityName, IsKey = true, 
            DefaultValue = TrackingConfigurationStrings.StarWildcard)]
        [StringValidator(MinLength = 1)]
        public string ChildActivityName
        {
            get { return (string)base[TrackingConfigurationStrings.ChildActivityName]; }
            set { base[TrackingConfigurationStrings.ChildActivityName] = value; }
        }

        protected override TrackingQuery NewTrackingQuery()
        {
            return new ActivityScheduledQuery
            {
                ActivityName = this.ActivityName,
                ChildActivityName = this.ChildActivityName
            };
        }
    }
}
