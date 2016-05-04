//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Tracking
{
    public sealed class ActivityScheduledQuery : TrackingQuery
    {
        public ActivityScheduledQuery()
        {
            this.ActivityName = "*";
            this.ChildActivityName = "*";
        }

        public string ActivityName { get; set; }
        public string ChildActivityName { get; set; }

    }
}
