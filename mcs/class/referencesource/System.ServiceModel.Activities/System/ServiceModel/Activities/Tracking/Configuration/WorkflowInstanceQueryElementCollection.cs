//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System.Runtime;
    using System.Configuration;

    [Fx.Tag.XamlVisible(false)]
    [ConfigurationCollection(typeof(WorkflowInstanceQueryElement),
        CollectionType = ConfigurationElementCollectionType.BasicMap,
        AddItemName = TrackingConfigurationStrings.WorkflowInstanceQuery)]
    public sealed class WorkflowInstanceQueryElementCollection : TrackingConfigurationCollection<WorkflowInstanceQueryElement>
    {
        protected override string ElementName
        {
            get { return TrackingConfigurationStrings.WorkflowInstanceQuery; }
        }
    }
}
