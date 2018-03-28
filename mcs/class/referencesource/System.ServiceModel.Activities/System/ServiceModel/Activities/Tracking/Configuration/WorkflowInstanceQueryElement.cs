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
    public class WorkflowInstanceQueryElement : TrackingQueryElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty(TrackingConfigurationStrings.States, typeof(System.ServiceModel.Activities.Tracking.Configuration.StateElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty(TrackingConfigurationStrings.States)]
        public StateElementCollection States
        {
            get { return (StateElementCollection)base[TrackingConfigurationStrings.States]; }
        }

        protected override TrackingQuery NewTrackingQuery()
        {
            WorkflowInstanceQuery query = new WorkflowInstanceQuery();

            foreach (StateElement stateElement in this.States)
            {
                query.States.Add(stateElement.Name);
            }

            return query;
        }
    }
}
