//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System.Configuration;
    using System.Activities.Tracking;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Diagnostics.CodeAnalysis;

    // Base class for all the workflow tracking query configuration elements
    [Fx.Tag.XamlVisible(false)]
    public abstract class TrackingQueryElement : TrackingConfigurationElement
    {
        ConfigurationPropertyCollection properties;
        Guid? elementKey;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty(TrackingConfigurationStrings.Annotations, typeof(System.ServiceModel.Activities.Tracking.Configuration.AnnotationElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty(TrackingConfigurationStrings.Annotations)]
        public AnnotationElementCollection Annotations
        {
            get
            {
                return (AnnotationElementCollection)base[TrackingConfigurationStrings.Annotations];
            }
        }

        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationPropertyAttributeRule,
            Justification = "This property is defined by the base class to compute unique key.")]
        public override object ElementKey
        {
            get
            {
                if (this.elementKey == null)
                {
                    this.elementKey = Guid.NewGuid();
                }
                return this.elementKey;
            }
        }


        internal TrackingQuery CreateTrackingQuery()
        {
            TrackingQuery query = NewTrackingQuery();
            UpdateTrackingQuery(query);
            return query;
        }

        // Override this method to create a query instance and set properties not inherited by derived classes
        protected abstract TrackingQuery NewTrackingQuery();

        // Override this method to set the properties that a derived class may inherit calling the base method
        protected virtual void UpdateTrackingQuery(TrackingQuery trackingQuery)
        {
            foreach (AnnotationElement annotation in this.Annotations)
            {
                trackingQuery.QueryAnnotations.Add(new KeyValuePair<string, string>(annotation.Name, annotation.Value));
            } 
        }
    }
}
