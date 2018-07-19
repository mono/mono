//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System.Runtime;
    using System.Configuration;

    [Fx.Tag.XamlVisible(false)]
    [ConfigurationCollection(typeof(AnnotationElement),
        CollectionType = ConfigurationElementCollectionType.BasicMap,
        AddItemName = TrackingConfigurationStrings.Annotation)]
    public class AnnotationElementCollection : TrackingConfigurationCollection<AnnotationElement>
    {
        protected override string ElementName
        {
            get { return TrackingConfigurationStrings.Annotation; }
        }
    }
}
