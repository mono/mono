//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Tracking
{
    class TrackingRecordPreFilter
    {
        public TrackingRecordPreFilter()
        {
        }

        public TrackingRecordPreFilter(bool trackingProviderInitialized)
        {
            if (trackingProviderInitialized)
            {
                this.TrackingProviderInitialized = true;
                this.TrackActivityScheduledRecords = true;
                this.TrackActivityStateRecords = true;
                this.TrackActivityStateRecordsClosedState = true;
                this.TrackActivityStateRecordsExecutingState = true;
                this.TrackBookmarkResumptionRecords = true;
                this.TrackCancelRequestedRecords = true;
                this.TrackFaultPropagationRecords = true;
                this.TrackWorkflowInstanceRecords = true;
            }
        }

        internal bool TrackingProviderInitialized
        {
            get;
            private set;
        }

        internal bool TrackWorkflowInstanceRecords
        {
            get;
            set;
        }

        internal bool TrackBookmarkResumptionRecords
        {
            get;
            set;
        }

        internal bool TrackActivityScheduledRecords
        {
            get;
            set;
        }

        internal bool TrackActivityStateRecordsClosedState
        {
            get;
            set;
        }

        internal bool TrackActivityStateRecordsExecutingState
        {
            get;
            set;
        }

        internal bool TrackActivityStateRecords
        {
            get;
            set;
        }
        
        internal bool TrackCancelRequestedRecords
        {
            get;
            set;
        }

        internal bool TrackFaultPropagationRecords
        {
            get;
            set;
        }

        internal void Merge(TrackingRecordPreFilter filter)
        {
            if (this.TrackingProviderInitialized)
            {
                this.TrackingProviderInitialized = false;
                this.TrackActivityStateRecordsExecutingState = filter.TrackActivityStateRecordsExecutingState;
                this.TrackActivityScheduledRecords = filter.TrackActivityScheduledRecords;
                this.TrackActivityStateRecords = filter.TrackActivityStateRecords;
                this.TrackActivityStateRecordsClosedState = filter.TrackActivityStateRecordsClosedState;
                this.TrackBookmarkResumptionRecords = filter.TrackBookmarkResumptionRecords;
                this.TrackCancelRequestedRecords = filter.TrackCancelRequestedRecords;
                this.TrackFaultPropagationRecords = filter.TrackFaultPropagationRecords;
                this.TrackWorkflowInstanceRecords = filter.TrackWorkflowInstanceRecords;
            }
            else
            {
                this.TrackActivityStateRecordsExecutingState |= filter.TrackActivityStateRecordsExecutingState;
                this.TrackActivityScheduledRecords |= filter.TrackActivityScheduledRecords;
                this.TrackActivityStateRecords |= filter.TrackActivityStateRecords;
                this.TrackActivityStateRecordsClosedState |= filter.TrackActivityStateRecordsClosedState;
                this.TrackBookmarkResumptionRecords |= filter.TrackBookmarkResumptionRecords;
                this.TrackCancelRequestedRecords |= filter.TrackCancelRequestedRecords;
                this.TrackFaultPropagationRecords |= filter.TrackFaultPropagationRecords;
                this.TrackWorkflowInstanceRecords |= filter.TrackWorkflowInstanceRecords;
            }
        }
    }
}
