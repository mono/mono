//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Tracking
{
    public class CustomTrackingQuery : TrackingQuery
    {

        public CustomTrackingQuery()
        {
        }

        public string Name
        {
            get;
            set;
        }

        public string ActivityName
        {
            get;
            set;
        }
    }
}
