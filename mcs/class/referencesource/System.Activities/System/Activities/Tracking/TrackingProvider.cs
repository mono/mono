//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Tracking
{
    using System;
    using System.Activities;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime;

    class TrackingProvider
    {
        List<TrackingParticipant> trackingParticipants;
        Dictionary<TrackingParticipant, RuntimeTrackingProfile> profileSubscriptions;
        IList<TrackingRecord> pendingTrackingRecords;
        Activity definition;
        bool filterValuesSetExplicitly;
        Hashtable activitySubscriptions;

        long nextTrackingRecordNumber;

        public TrackingProvider(Activity definition)
        {
            this.definition = definition;
            this.ShouldTrack = true;
            this.ShouldTrackActivityStateRecords = true;
            this.ShouldTrackActivityStateRecordsExecutingState = true;
            this.ShouldTrackActivityStateRecordsClosedState = true;
            this.ShouldTrackBookmarkResumptionRecords = true;
            this.ShouldTrackActivityScheduledRecords = true;
            this.ShouldTrackCancelRequestedRecords = true;
            this.ShouldTrackFaultPropagationRecords = true;
            this.ShouldTrackWorkflowInstanceRecords = true;
        }

        public bool HasPendingRecords
        {
            get
            {
                return (this.pendingTrackingRecords != null && this.pendingTrackingRecords.Count > 0)
                    || !this.filterValuesSetExplicitly;
            }
        }

        public long NextTrackingRecordNumber
        {
            get
            {
                return this.nextTrackingRecordNumber;
            }
        }

        public bool ShouldTrack
        {
            get;
            private set;
        }

        public bool ShouldTrackWorkflowInstanceRecords
        {
            get;
            private set;
        }

        public bool ShouldTrackBookmarkResumptionRecords
        {
            get;
            private set;
        }

        public bool ShouldTrackActivityScheduledRecords
        {
            get;
            private set;
        }

        public bool ShouldTrackActivityStateRecords
        {
            get;
            private set;
        }

        public bool ShouldTrackActivityStateRecordsExecutingState
        {
            get;
            private set;
        }

        public bool ShouldTrackActivityStateRecordsClosedState
        {
            get;
            private set;
        }

        public bool ShouldTrackCancelRequestedRecords
        {
            get;
            private set;
        }

        public bool ShouldTrackFaultPropagationRecords
        {
            get;
            private set;
        }

        long GetNextRecordNumber()
        {
            // We blindly do this.  On the off chance that a workflow causes it to loop back
            // around it shouldn't cause the workflow to fail and the tracking information
            // will still be salvagable.
            return this.nextTrackingRecordNumber++;
        }

        public void OnDeserialized(long nextTrackingRecordNumber)
        {
            this.nextTrackingRecordNumber = nextTrackingRecordNumber;
        }

        public void AddRecord(TrackingRecord record)
        {
            if (this.pendingTrackingRecords == null)
            {
                this.pendingTrackingRecords = new List<TrackingRecord>();
            }

            record.RecordNumber = GetNextRecordNumber();
            this.pendingTrackingRecords.Add(record);
        }

        public void AddParticipant(TrackingParticipant participant)
        {
            if (this.trackingParticipants == null)
            {
                this.trackingParticipants = new List<TrackingParticipant>();
                this.profileSubscriptions = new Dictionary<TrackingParticipant, RuntimeTrackingProfile>();
            }
            this.trackingParticipants.Add(participant);
        }

        public void ClearParticipants()
        {
            this.trackingParticipants = null;
            this.profileSubscriptions = null;
        }

        public void FlushPendingRecords(TimeSpan timeout)
        {
            try
            {
                if (this.HasPendingRecords)
                {
                    TimeoutHelper helper = new TimeoutHelper(timeout);
                    for (int i = 0; i < this.trackingParticipants.Count; i++)
                    {
                        TrackingParticipant participant = this.trackingParticipants[i];
                        RuntimeTrackingProfile runtimeProfile = GetRuntimeTrackingProfile(participant);

                        // HasPendingRecords can be true for the sole purpose of populating our initial profiles, so check again here
                        if (this.pendingTrackingRecords != null)
                        {
                            for (int j = 0; j < this.pendingTrackingRecords.Count; j++)
                            {
                                TrackingRecord currentRecord = this.pendingTrackingRecords[j];
                                Fx.Assert(currentRecord != null, "We should never come across a null context.");

                                TrackingRecord preparedRecord = null;
                                bool shouldClone = this.trackingParticipants.Count > 1;
                                if (runtimeProfile == null)
                                {
                                    preparedRecord = shouldClone ? currentRecord.Clone() : currentRecord;
                                }
                                else
                                {
                                    preparedRecord = runtimeProfile.Match(currentRecord, shouldClone);
                                }

                                if (preparedRecord != null)
                                {
                                    participant.Track(preparedRecord, helper.RemainingTime());
                                    if (TD.TrackingRecordRaisedIsEnabled())
                                    {
                                        TD.TrackingRecordRaised(preparedRecord.ToString(), participant.GetType().ToString());
                                    }
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                // Note that if we fail to track yet the workflow manages to recover
                // we will attempt to track those records again.
                ClearPendingRecords();
            }
        }

        public IAsyncResult BeginFlushPendingRecords(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new FlushPendingRecordsAsyncResult(this, timeout, callback, state);
        }

        public void EndFlushPendingRecords(IAsyncResult result)
        {
            FlushPendingRecordsAsyncResult.End(result);
        }

        public bool ShouldTrackActivity(string name)
        {
            return this.activitySubscriptions == null || this.activitySubscriptions.ContainsKey(name) || this.activitySubscriptions.ContainsKey("*");
        }

        void ClearPendingRecords()
        {
            if (this.pendingTrackingRecords != null)
            {
                //since the number of records is small, it is faster to remove from end than to call List.Clear
                for (int i = this.pendingTrackingRecords.Count - 1; i >= 0; i--)
                {
                    this.pendingTrackingRecords.RemoveAt(i);
                }
            }
        }

        RuntimeTrackingProfile GetRuntimeTrackingProfile(TrackingParticipant participant)
        {
            TrackingProfile profile;
            RuntimeTrackingProfile runtimeProfile;

            if (!this.profileSubscriptions.TryGetValue(participant, out runtimeProfile))
            {
                profile = participant.TrackingProfile;

                if (profile != null)
                {
                    runtimeProfile = RuntimeTrackingProfile.GetRuntimeTrackingProfile(profile, this.definition);
                    Merge(runtimeProfile.Filter);

                    //Add the names to the list of activities that have subscriptions.  This provides a quick lookup
                    //for the runtime to check if a TrackingRecord has to be created. 
                    IEnumerable<string> activityNames = runtimeProfile.GetSubscribedActivityNames();
                    if (activityNames != null)
                    {
                        if (this.activitySubscriptions == null)
                        {
                            this.activitySubscriptions = new Hashtable();
                        }
                        foreach (string name in activityNames)
                        {
                            if (this.activitySubscriptions[name] == null)
                            {
                                this.activitySubscriptions[name] = name;
                            }
                        }
                    }
                }
                else
                {
                    //for null profiles, set all the filter flags. 
                    Merge(new TrackingRecordPreFilter(true));
                }

                this.profileSubscriptions.Add(participant, runtimeProfile);
            }
            return runtimeProfile;
        }

        void Merge(TrackingRecordPreFilter filter)
        {
            if (!this.filterValuesSetExplicitly)
            {
                // This it the first filter we are merging
                this.filterValuesSetExplicitly = true;

                this.ShouldTrackActivityStateRecordsExecutingState = filter.TrackActivityStateRecordsExecutingState;
                this.ShouldTrackActivityScheduledRecords = filter.TrackActivityScheduledRecords;
                this.ShouldTrackActivityStateRecords = filter.TrackActivityStateRecords;
                this.ShouldTrackActivityStateRecordsClosedState = filter.TrackActivityStateRecordsClosedState;
                this.ShouldTrackBookmarkResumptionRecords = filter.TrackBookmarkResumptionRecords;
                this.ShouldTrackCancelRequestedRecords = filter.TrackCancelRequestedRecords;
                this.ShouldTrackFaultPropagationRecords = filter.TrackFaultPropagationRecords;
                this.ShouldTrackWorkflowInstanceRecords = filter.TrackWorkflowInstanceRecords;
            }
            else
            {
                this.ShouldTrackActivityStateRecordsExecutingState |= filter.TrackActivityStateRecordsExecutingState;
                this.ShouldTrackActivityScheduledRecords |= filter.TrackActivityScheduledRecords;
                this.ShouldTrackActivityStateRecords |= filter.TrackActivityStateRecords;
                this.ShouldTrackActivityStateRecordsClosedState |= filter.TrackActivityStateRecordsClosedState;
                this.ShouldTrackBookmarkResumptionRecords |= filter.TrackBookmarkResumptionRecords;
                this.ShouldTrackCancelRequestedRecords |= filter.TrackCancelRequestedRecords;
                this.ShouldTrackFaultPropagationRecords |= filter.TrackFaultPropagationRecords;
                this.ShouldTrackWorkflowInstanceRecords |= filter.TrackWorkflowInstanceRecords;
            }
        }

        class FlushPendingRecordsAsyncResult : AsyncResult
        {
            static AsyncCompletion trackingCompleteCallback = new AsyncCompletion(OnTrackingComplete);

            int currentRecord;
            int currentParticipant;
            TrackingProvider provider;
            TimeoutHelper timeoutHelper;

            public FlushPendingRecordsAsyncResult(TrackingProvider provider, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.provider = provider;
                this.timeoutHelper = new TimeoutHelper(timeout);

                if (RunLoop())
                {
                    Complete(true);
                }
            }

            bool RunLoop()
            {
                if (this.provider.HasPendingRecords)
                {
                    while (this.currentParticipant < this.provider.trackingParticipants.Count)
                    {
                        TrackingParticipant participant = this.provider.trackingParticipants[this.currentParticipant];
                        RuntimeTrackingProfile runtimeProfile = this.provider.GetRuntimeTrackingProfile(participant);

                        if (this.provider.pendingTrackingRecords != null)
                        {
                            while (this.currentRecord < this.provider.pendingTrackingRecords.Count)
                            {
                                bool completedSynchronously = PostTrackingRecord(participant, runtimeProfile);
                                if (!completedSynchronously)
                                {
                                    return false;
                                }
                            }
                        }

                        this.currentRecord = 0;
                        this.currentParticipant++;
                    }
                }

                // We've now tracked all of the records.
                this.provider.ClearPendingRecords();
                return true;
            }

            static bool OnTrackingComplete(IAsyncResult result)
            {
                Fx.Assert(!result.CompletedSynchronously, "TrackingAsyncResult.OnTrackingComplete should not get called with a result that is CompletedSynchronously");

                FlushPendingRecordsAsyncResult thisPtr = (FlushPendingRecordsAsyncResult)result.AsyncState;
                TrackingParticipant participant = thisPtr.provider.trackingParticipants[thisPtr.currentParticipant];
                bool isSuccessful = false;
                try
                {
                    participant.EndTrack(result);
                    isSuccessful = true;
                }
                finally
                {
                    if (!isSuccessful)
                    {
                        thisPtr.provider.ClearPendingRecords();
                    }
                }
                return thisPtr.RunLoop();
            }

            bool PostTrackingRecord(TrackingParticipant participant, RuntimeTrackingProfile runtimeProfile)
            {
                TrackingRecord originalRecord = this.provider.pendingTrackingRecords[this.currentRecord];
                this.currentRecord++;
                bool isSuccessful = false;

                try
                {
                    TrackingRecord preparedRecord = null;
                    bool shouldClone = this.provider.trackingParticipants.Count > 1;
                    if (runtimeProfile == null)
                    {
                        preparedRecord = shouldClone ? originalRecord.Clone() : originalRecord;
                    }
                    else
                    {
                        preparedRecord = runtimeProfile.Match(originalRecord, shouldClone);
                    }

                    if (preparedRecord != null)
                    {
                        IAsyncResult result = participant.BeginTrack(preparedRecord, this.timeoutHelper.RemainingTime(), PrepareAsyncCompletion(trackingCompleteCallback), this);
                        if (TD.TrackingRecordRaisedIsEnabled())
                        {
                            TD.TrackingRecordRaised(preparedRecord.ToString(), participant.GetType().ToString());
                        }
                        if (result.CompletedSynchronously)
                        {
                            participant.EndTrack(result);
                        }
                        else
                        {
                            isSuccessful = true;
                            return false;
                        }
                    }
                    isSuccessful = true;
                }
                finally
                {
                    if (!isSuccessful)
                    {
                        this.provider.ClearPendingRecords();
                    }
                }
                return true;
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<FlushPendingRecordsAsyncResult>(result);
            }
        }
    }
}
