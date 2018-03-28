//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System.Configuration;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.Activities.Tracking;
    using System.Diagnostics.CodeAnalysis;

    [Fx.Tag.XamlVisible(false)]
    public class ProfileWorkflowElement : TrackingConfigurationElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty(TrackingConfigurationStrings.ActivityDefinitionId, typeof(System.String), "*", null, new System.Configuration.StringValidator(1, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsKey));
                    properties.Add(new ConfigurationProperty(TrackingConfigurationStrings.WorkflowInstanceQueries, typeof(System.ServiceModel.Activities.Tracking.Configuration.WorkflowInstanceQueryElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty(TrackingConfigurationStrings.ActivityQueries, typeof(System.ServiceModel.Activities.Tracking.Configuration.ActivityStateQueryElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty(TrackingConfigurationStrings.ActivityScheduledQueries, typeof(System.ServiceModel.Activities.Tracking.Configuration.ActivityScheduledQueryElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty(TrackingConfigurationStrings.CancelRequestedQueries, typeof(System.ServiceModel.Activities.Tracking.Configuration.CancelRequestedQueryElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty(TrackingConfigurationStrings.FaultPropagationQueries, typeof(System.ServiceModel.Activities.Tracking.Configuration.FaultPropagationQueryElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty(TrackingConfigurationStrings.BookmarkResumptionQueries, typeof(System.ServiceModel.Activities.Tracking.Configuration.BookmarkResumptionQueryElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty(TrackingConfigurationStrings.CustomTrackingQueries, typeof(System.ServiceModel.Activities.Tracking.Configuration.CustomTrackingQueryElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty(TrackingConfigurationStrings.StateMachineStateQueries, typeof(System.ServiceModel.Activities.Tracking.Configuration.StateMachineStateQueryElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }

        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationPropertyAttributeRule,
            Justification = "This property is defined by the base class to compute unique key.")]
        public override object ElementKey
        {
            get { return this.ActivityDefinitionId; }
        }

        [ConfigurationProperty(TrackingConfigurationStrings.ActivityDefinitionId, IsKey = true,
            DefaultValue = TrackingConfigurationStrings.StarWildcard)]
        [StringValidator(MinLength = 1)]
        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule,
            MessageId = "System.ServiceModel.Activities.Tracking.Configuration.ProfileWorkflowElement.ActivityDefinitionId",
            Justification = "StringValidator verifies minimum size")]
        public string ActivityDefinitionId
        {
            get { return (string)base[TrackingConfigurationStrings.ActivityDefinitionId]; }
            set { base[TrackingConfigurationStrings.ActivityDefinitionId] = value; }
        }

        [ConfigurationProperty(TrackingConfigurationStrings.WorkflowInstanceQueries)]
        public WorkflowInstanceQueryElementCollection WorkflowInstanceQueries
        {
            get
            {
                return (WorkflowInstanceQueryElementCollection)
                    base[TrackingConfigurationStrings.WorkflowInstanceQueries];
            }
        }

        [ConfigurationProperty(TrackingConfigurationStrings.ActivityQueries)]
        public ActivityStateQueryElementCollection ActivityStateQueries
        {
            get { return (ActivityStateQueryElementCollection)base[TrackingConfigurationStrings.ActivityQueries]; }
        }


        [ConfigurationProperty(TrackingConfigurationStrings.ActivityScheduledQueries)]
        public ActivityScheduledQueryElementCollection ActivityScheduledQueries
        {
            get
            {
                return (ActivityScheduledQueryElementCollection)
                    base[TrackingConfigurationStrings.ActivityScheduledQueries];
            }
        }

        [ConfigurationProperty(TrackingConfigurationStrings.CancelRequestedQueries)]
        public CancelRequestedQueryElementCollection CancelRequestedQueries
        {
            get
            {
                return (CancelRequestedQueryElementCollection)
                    base[TrackingConfigurationStrings.CancelRequestedQueries];
            }
        }

        [ConfigurationProperty(TrackingConfigurationStrings.FaultPropagationQueries)]
        public FaultPropagationQueryElementCollection FaultPropagationQueries
        {
            get
            {
                return (FaultPropagationQueryElementCollection)
                    base[TrackingConfigurationStrings.FaultPropagationQueries];
            }
        }

        [ConfigurationProperty(TrackingConfigurationStrings.BookmarkResumptionQueries)]
        public BookmarkResumptionQueryElementCollection BookmarkResumptionQueries
        {
            get
            {
                return (BookmarkResumptionQueryElementCollection)
                    base[TrackingConfigurationStrings.BookmarkResumptionQueries];
            }
        }

        [ConfigurationProperty(TrackingConfigurationStrings.CustomTrackingQueries)]
        public CustomTrackingQueryElementCollection CustomTrackingQueries
        {
            get
            {
                return (CustomTrackingQueryElementCollection)
                    base[TrackingConfigurationStrings.CustomTrackingQueries];
            }
        }
        
        [ConfigurationProperty(TrackingConfigurationStrings.StateMachineStateQueries)]
        public StateMachineStateQueryElementCollection StateMachineStateQueries
        {
            get
            {
                return (StateMachineStateQueryElementCollection)
                    base[TrackingConfigurationStrings.StateMachineStateQueries];
            }
        }

        internal void AddQueries(Collection<TrackingQuery> queries) 
        {
            AddQueryCollection(queries, this.WorkflowInstanceQueries);
            AddQueryCollection(queries, this.ActivityStateQueries);
            AddQueryCollection(queries, this.ActivityScheduledQueries);
            AddQueryCollection(queries, this.CancelRequestedQueries);
            AddQueryCollection(queries, this.FaultPropagationQueries);
            AddQueryCollection(queries, this.BookmarkResumptionQueries);
            AddQueryCollection(queries, this.CustomTrackingQueries);
            AddQueryCollection(queries, this.StateMachineStateQueries);
        }

        static void AddQueryCollection(Collection<TrackingQuery> queries, ConfigurationElementCollection elements)
        {
            foreach (TrackingQueryElement queryElement in elements)
            {
                queries.Add(queryElement.CreateTrackingQuery());
            }
        }
    }
}
