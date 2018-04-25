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
    public class FaultPropagationQueryElement : TrackingQueryElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty(TrackingConfigurationStrings.FaultSourceActivityName, typeof(System.String), "*", null, new System.Configuration.StringValidator(1, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsKey));
                    properties.Add(new ConfigurationProperty(TrackingConfigurationStrings.FaultHandlerActivityName, typeof(System.String), "*", null, new System.Configuration.StringValidator(1, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsKey));
                    this.properties = properties;
                }
                return this.properties;
            }
        }

        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule,
            MessageId = "System.ServiceModel.Activities.Tracking.Configuration.ActivityScheduledQueryElement.ChildActivityName",
            Justification = "StringValidator verifies minimum size")]
        [ConfigurationProperty(TrackingConfigurationStrings.FaultSourceActivityName, IsKey = true,
            DefaultValue = TrackingConfigurationStrings.StarWildcard)]
        [StringValidator(MinLength = 1)]
        public string FaultSourceActivityName
        {
            get { return (string)base[TrackingConfigurationStrings.FaultSourceActivityName]; }
            set { base[TrackingConfigurationStrings.FaultSourceActivityName] = value; }
        }
        
        [ConfigurationProperty(TrackingConfigurationStrings.FaultHandlerActivityName, IsKey = true,
            DefaultValue = TrackingConfigurationStrings.StarWildcard)]
        [StringValidator(MinLength = 1)]
        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule,
            MessageId = "System.ServiceModel.Activities.Tracking.Configuration.FaultPropagationQueryElement.FaultHandlerActivityName",
            Justification = "StringValidator verifies minimum size")]
        public string FaultHandlerActivityName
        {
            get { return (string)base[TrackingConfigurationStrings.FaultHandlerActivityName]; }
            set { base[TrackingConfigurationStrings.FaultHandlerActivityName] = value; }
        }

        protected override TrackingQuery NewTrackingQuery()
        {
            FaultPropagationQuery query = new FaultPropagationQuery
                {
                    FaultSourceActivityName = this.FaultSourceActivityName,
                    FaultHandlerActivityName = this.FaultHandlerActivityName
                };

            return query;
        }
    }
}
