//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System.Runtime;
    using System.Configuration;

    [Fx.Tag.XamlVisible(false)]
    [ConfigurationCollection(typeof(VariableElement),
        CollectionType = ConfigurationElementCollectionType.BasicMap,
        AddItemName = TrackingConfigurationStrings.VariableQuery)]
    public class VariableElementCollection : TrackingConfigurationCollection<VariableElement>
    {
        protected override string ElementName
        {
            get { return TrackingConfigurationStrings.VariableQuery; }
        }
    }
}
