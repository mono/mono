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
    public class ActivityStateQueryElement : TrackingQueryElement
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
                    properties.Add(new ConfigurationProperty(TrackingConfigurationStrings.States, typeof(System.ServiceModel.Activities.Tracking.Configuration.StateElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty(TrackingConfigurationStrings.VariableQueries, typeof(System.ServiceModel.Activities.Tracking.Configuration.VariableElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty(TrackingConfigurationStrings.ArgumentQueries, typeof(System.ServiceModel.Activities.Tracking.Configuration.ArgumentElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }

        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule,
            MessageId = "System.ServiceModel.Activities.Tracking.Configuration.ActivityQueryElementBase.ActivityName",
            Justification = "StringValidator verifies minimum size")]
        [ConfigurationProperty(TrackingConfigurationStrings.ActivityName, IsKey = true,
            DefaultValue = TrackingConfigurationStrings.StarWildcard)]
        [StringValidator(MinLength = 1)]
        public string ActivityName
        {
            get { return (string)base[TrackingConfigurationStrings.ActivityName]; }
            set { base[TrackingConfigurationStrings.ActivityName] = value; }
        }

        [ConfigurationProperty(TrackingConfigurationStrings.States)]
        public StateElementCollection States
        {
            get { return (StateElementCollection)base[TrackingConfigurationStrings.States]; }
        }        

        [ConfigurationProperty(TrackingConfigurationStrings.VariableQueries)]
        public VariableElementCollection Variables
        {
            get
            {
                return (VariableElementCollection)base[TrackingConfigurationStrings.VariableQueries];
            }
        }

        [ConfigurationProperty(TrackingConfigurationStrings.ArgumentQueries)]
        public ArgumentElementCollection Arguments
        {
            get
            {
                return (ArgumentElementCollection)base[TrackingConfigurationStrings.ArgumentQueries];
            }
        }
        
        protected override TrackingQuery NewTrackingQuery()
        {
            ActivityStateQuery query = new ActivityStateQuery()
            {
                ActivityName = this.ActivityName
            };

            foreach (StateElement stateElement in this.States)
            {
                query.States.Add(stateElement.Name);
            }

            foreach (VariableElement variableQueryElement in this.Variables)
            {
                query.Variables.Add(variableQueryElement.Name);
            }

            foreach (ArgumentElement argumentQueryElement in this.Arguments)
            {
                query.Arguments.Add(argumentQueryElement.Name);
            }

            return query;
        }
    }
}
