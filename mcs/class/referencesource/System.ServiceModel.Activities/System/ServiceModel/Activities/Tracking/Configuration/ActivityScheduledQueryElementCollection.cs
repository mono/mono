//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System.Runtime;
    using System.Configuration;

    [Fx.Tag.XamlVisible(false)]
    [ConfigurationCollection(typeof(ActivityScheduledQueryElement),
        CollectionType = ConfigurationElementCollectionType.BasicMap,
        AddItemName = TrackingConfigurationStrings.ActivityScheduledQuery)]
    public class ActivityScheduledQueryElementCollection : TrackingConfigurationCollection<ActivityScheduledQueryElement>
    {
        protected override string ElementName
        {
            get { return TrackingConfigurationStrings.ActivityScheduledQuery; }
        }
    }
}
