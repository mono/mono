//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Tracking
{
    public sealed class CancelRequestedQuery : TrackingQuery
    {
        public CancelRequestedQuery()
        {
            this.ActivityName = "*";
            this.ChildActivityName = "*";
        }

        public string ActivityName { get; set; }
        public string ChildActivityName { get; set; }

    }
}
