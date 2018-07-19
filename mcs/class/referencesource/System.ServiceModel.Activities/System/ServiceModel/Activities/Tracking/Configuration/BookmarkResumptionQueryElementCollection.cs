//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System.Runtime;
    using System.Configuration;

    [Fx.Tag.XamlVisible(false)]
    [ConfigurationCollection(typeof(BookmarkResumptionQueryElement),
        CollectionType = ConfigurationElementCollectionType.BasicMap,
        AddItemName = TrackingConfigurationStrings.BookmarkResumptionQuery)]
    public class BookmarkResumptionQueryElementCollection : TrackingConfigurationCollection<BookmarkResumptionQueryElement>
    {
        protected override string ElementName
        {
            get { return TrackingConfigurationStrings.BookmarkResumptionQuery; }
        }
    }
}
