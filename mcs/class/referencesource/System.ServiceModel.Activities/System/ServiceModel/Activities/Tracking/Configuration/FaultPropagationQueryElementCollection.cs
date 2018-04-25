//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System.Runtime;
    using System.Configuration;

    [Fx.Tag.XamlVisible(false)]
    [ConfigurationCollection(typeof(FaultPropagationQueryElement),
        CollectionType = ConfigurationElementCollectionType.BasicMap,
        AddItemName = TrackingConfigurationStrings.FaultPropagationQuery)]
    public class FaultPropagationQueryElementCollection : TrackingConfigurationCollection<FaultPropagationQueryElement>
    {
        protected override string ElementName
        {
            get { return TrackingConfigurationStrings.FaultPropagationQuery; }
        }
    }
}
