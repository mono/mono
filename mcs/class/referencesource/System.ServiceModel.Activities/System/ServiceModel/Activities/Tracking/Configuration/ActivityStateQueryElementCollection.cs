//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System.Runtime;
    using System.Configuration;

    [Fx.Tag.XamlVisible(false)]
    [ConfigurationCollection(typeof(ActivityStateQueryElement),
        CollectionType = ConfigurationElementCollectionType.BasicMap,
        AddItemName = TrackingConfigurationStrings.ActivityQuery)]
    public class ActivityStateQueryElementCollection : TrackingConfigurationCollection<ActivityStateQueryElement>
    {
        protected override string ElementName
        {
            get { return TrackingConfigurationStrings.ActivityQuery; }
        }
    }
}
