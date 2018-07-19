//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Tracking
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Markup;

    [ContentProperty("Queries")]
    public class TrackingProfile
    {
        Collection<TrackingQuery> queries;

        public TrackingProfile()
        {
        }

        [DefaultValue(null)]
        public string Name { get; set; }

        [DefaultValue(ImplementationVisibility.RootScope)]
        public ImplementationVisibility ImplementationVisibility { get; set; }

        [DefaultValue(null)]
        public string ActivityDefinitionId { get; set; }

        public Collection<TrackingQuery> Queries
        {
            get
            {
                if (this.queries == null)
                {
                    this.queries = new Collection<TrackingQuery>();
                }
                return this.queries;
            }
        }
    }
}
