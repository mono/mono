//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System.Runtime;
    using System.Configuration;

    [Fx.Tag.XamlVisible(false)]
    [ConfigurationCollection(typeof(CustomTrackingQueryElement),
        CollectionType = ConfigurationElementCollectionType.BasicMap,
        AddItemName = TrackingConfigurationStrings.CustomTrackingQuery)]
    public class CustomTrackingQueryElementCollection : TrackingConfigurationCollection<CustomTrackingQueryElement>
    {
        protected override string ElementName
        {
            get { return TrackingConfigurationStrings.CustomTrackingQuery; }
        }
    }
}
