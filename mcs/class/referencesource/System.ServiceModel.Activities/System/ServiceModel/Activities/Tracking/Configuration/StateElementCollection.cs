//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System.Runtime;
    using System.Configuration;

    [Fx.Tag.XamlVisible(false)]
    [ConfigurationCollection(typeof(StateElement),
        CollectionType = ConfigurationElementCollectionType.BasicMap,
        AddItemName = TrackingConfigurationStrings.State)]
    public sealed class StateElementCollection : TrackingConfigurationCollection<StateElement>
    {
        protected override string ElementName
        {
            get { return TrackingConfigurationStrings.State; }
        }
    }
}
