//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Activities.Tracking;

    [Obsolete("The WF3 Types are deprecated. Instead, please use the new WF4 Types from System.Activities.*")] 
    public class InteropTrackingRecord : CustomTrackingRecord
    {
        public System.Workflow.Runtime.Tracking.TrackingRecord TrackingRecord { get; private set; }

        public InteropTrackingRecord(string activityDisplayName,
            System.Workflow.Runtime.Tracking.TrackingRecord v1TrackingRecord)
            : base(activityDisplayName)
        {
            this.TrackingRecord = v1TrackingRecord;
            this.Data.Add("TrackingRecord", v1TrackingRecord);
        }

        protected InteropTrackingRecord(InteropTrackingRecord record)
            : base(record)
        {
            this.TrackingRecord = record.TrackingRecord;
        }

        protected override TrackingRecord Clone()
        {
            return new InteropTrackingRecord(this);
        }
    }
}
