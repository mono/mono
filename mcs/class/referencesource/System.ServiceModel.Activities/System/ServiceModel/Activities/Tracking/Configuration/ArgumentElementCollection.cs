//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System.Runtime;
    using System.Configuration;

    [Fx.Tag.XamlVisible(false)]
    [ConfigurationCollection(typeof(ArgumentElement),
        CollectionType = ConfigurationElementCollectionType.BasicMap,
        AddItemName = TrackingConfigurationStrings.ArgumentQuery)]
    public class ArgumentElementCollection : TrackingConfigurationCollection<ArgumentElement>
    {
        protected override string ElementName
        {
            get { return TrackingConfigurationStrings.ArgumentQuery; }
        }
    }
}
