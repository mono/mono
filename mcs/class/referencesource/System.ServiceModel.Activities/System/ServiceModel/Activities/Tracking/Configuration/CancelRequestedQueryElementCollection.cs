//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System.Runtime;
    using System.Configuration;

    [Fx.Tag.XamlVisible(false)]
    [ConfigurationCollection(typeof(CancelRequestedQueryElement),
        CollectionType = ConfigurationElementCollectionType.BasicMap,
        AddItemName = TrackingConfigurationStrings.CancelRequestedQuery)]
    public class CancelRequestedQueryElementCollection : TrackingConfigurationCollection<CancelRequestedQueryElement>
    {
        protected override string ElementName
        {
            get { return TrackingConfigurationStrings.CancelRequestedQuery; }
        }
    }
}
