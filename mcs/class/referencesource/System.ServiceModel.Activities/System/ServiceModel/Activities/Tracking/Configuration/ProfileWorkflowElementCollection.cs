//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System.Configuration;
    using System.Runtime;

    [Fx.Tag.XamlVisible(false)]
    [ConfigurationCollection(typeof(ProfileWorkflowElement),
        CollectionType = ConfigurationElementCollectionType.BasicMap,
        AddItemName = TrackingConfigurationStrings.Workflow)]
    public class ProfileWorkflowElementCollection : TrackingConfigurationCollection<ProfileWorkflowElement>
    {
        protected override string ElementName
        {
            get { return TrackingConfigurationStrings.Workflow; }
        }
    }
}
