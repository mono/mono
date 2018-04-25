//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Tracking
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Collections.Specialized;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    class RuntimeTrackingProfile
    {
        static RuntimeTrackingProfileCache profileCache;

        List<ActivityScheduledQuery> activityScheduledSubscriptions;
        List<FaultPropagationQuery> faultPropagationSubscriptions;
        List<CancelRequestedQuery> cancelRequestedSubscriptions;
        Dictionary<string, HybridCollection<ActivityStateQuery>> activitySubscriptions;
        List<CustomTrackingQuery> customTrackingQuerySubscriptions;
        Dictionary<string, BookmarkResumptionQuery> bookmarkSubscriptions;
        Dictionary<string, WorkflowInstanceQuery> workflowEventSubscriptions;

        TrackingProfile associatedProfile;
        TrackingRecordPreFilter trackingRecordPreFilter;
        List<string> activityNames;

        bool isRootNativeActivity;

        internal RuntimeTrackingProfile(TrackingProfile profile, Activity rootElement)
        {
            this.associatedProfile = profile;
            this.isRootNativeActivity = rootElement is NativeActivity;
            this.trackingRecordPreFilter = new TrackingRecordPreFilter();

            foreach (TrackingQuery query in this.associatedProfile.Queries)
            {
                if (query is ActivityStateQuery)
                {
                    AddActivitySubscription((ActivityStateQuery)query);
                }
                else if (query is WorkflowInstanceQuery)
                {
                    AddWorkflowSubscription((WorkflowInstanceQuery)query);
                }
                else if (query is BookmarkResumptionQuery)
                {
                    AddBookmarkSubscription((BookmarkResumptionQuery)query);
                }
                else if (query is CustomTrackingQuery)
                {
                    AddCustomTrackingSubscription((CustomTrackingQuery)query);
                }
                else if (query is ActivityScheduledQuery)
                {
                    AddActivityScheduledSubscription((ActivityScheduledQuery)query);
                }
                else if (query is CancelRequestedQuery)
                {
                    AddCancelRequestedSubscription((CancelRequestedQuery)query);
                }
                else if (query is FaultPropagationQuery)
                {
                    AddFaultPropagationSubscription((FaultPropagationQuery)query);
                }
            }
        }

        static RuntimeTrackingProfileCache Cache
        {
            get
            {
                // We do not take a lock here because a true singleton is not required.
                if (profileCache == null)
                {
                    profileCache = new RuntimeTrackingProfileCache();
                }
                return profileCache;
            }
        }

        internal TrackingRecordPreFilter Filter
        {
            get
            {
                return this.trackingRecordPreFilter;
            }
        }

        internal IEnumerable<string> GetSubscribedActivityNames()
        {
            return this.activityNames;
        }

        bool ShouldTrackActivity(ActivityInfo activityInfo, string queryName)
        {
            if (activityInfo != null && queryName == "*")
            {
                if (this.isRootNativeActivity)
                {
                    if (activityInfo.Activity.MemberOf.ParentId != 0)
                    {
                        return false;
                    }
                }
                else
                {
                    if ((activityInfo.Activity.MemberOf.ParentId != 0)
                        && (activityInfo.Activity.MemberOf.Parent.ParentId != 0))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        void AddActivityName(string name)
        {
            if (this.activityNames == null)
            {
                this.activityNames = new List<string>();
            }
            this.activityNames.Add(name);
        }

        internal static RuntimeTrackingProfile GetRuntimeTrackingProfile(TrackingProfile profile, Activity rootElement)
        {
            return RuntimeTrackingProfile.Cache.GetRuntimeTrackingProfile(profile, rootElement);
        }

        void AddActivitySubscription(ActivityStateQuery query)
        {
            this.trackingRecordPreFilter.TrackActivityStateRecords = true;

            foreach (string state in query.States)
            {
                if (string.CompareOrdinal(state, "*") == 0)
                {
                    this.trackingRecordPreFilter.TrackActivityStateRecordsClosedState = true;
                    this.trackingRecordPreFilter.TrackActivityStateRecordsExecutingState = true;
                    break;
                }
                if (string.CompareOrdinal(state, ActivityStates.Closed) == 0)
                {
                    this.trackingRecordPreFilter.TrackActivityStateRecordsClosedState = true;
                }
                else if (string.CompareOrdinal(state, ActivityStates.Executing) == 0)
                {
                    this.trackingRecordPreFilter.TrackActivityStateRecordsExecutingState = true;
                }
            }

            if (this.activitySubscriptions == null)
            {
                this.activitySubscriptions = new Dictionary<string, HybridCollection<ActivityStateQuery>>();
            }

            HybridCollection<ActivityStateQuery> subscription;
            if (!this.activitySubscriptions.TryGetValue(query.ActivityName, out subscription))
            {
                subscription = new HybridCollection<ActivityStateQuery>();
                this.activitySubscriptions[query.ActivityName] = subscription;
            }
            subscription.Add((ActivityStateQuery)query);
            AddActivityName(query.ActivityName);
        }

        void AddActivityScheduledSubscription(ActivityScheduledQuery activityScheduledQuery)
        {
            this.trackingRecordPreFilter.TrackActivityScheduledRecords = true;
            if (this.activityScheduledSubscriptions == null)
            {
                this.activityScheduledSubscriptions = new List<ActivityScheduledQuery>();
            }
            this.activityScheduledSubscriptions.Add(activityScheduledQuery);
        }

        void AddCancelRequestedSubscription(CancelRequestedQuery cancelQuery)
        {
            this.trackingRecordPreFilter.TrackCancelRequestedRecords = true;
            if (this.cancelRequestedSubscriptions == null)
            {
                this.cancelRequestedSubscriptions = new List<CancelRequestedQuery>();
            }
            this.cancelRequestedSubscriptions.Add(cancelQuery);
        }

        void AddFaultPropagationSubscription(FaultPropagationQuery faultQuery)
        {
            this.trackingRecordPreFilter.TrackFaultPropagationRecords = true;
            if (this.faultPropagationSubscriptions == null)
            {
                this.faultPropagationSubscriptions = new List<FaultPropagationQuery>();
            }
            this.faultPropagationSubscriptions.Add(faultQuery);
        }

        void AddBookmarkSubscription(BookmarkResumptionQuery bookmarkTrackingQuery)
        {
            this.trackingRecordPreFilter.TrackBookmarkResumptionRecords = true;
            if (this.bookmarkSubscriptions == null)
            {
                this.bookmarkSubscriptions = new Dictionary<string, BookmarkResumptionQuery>();
            }
            //if duplicates are found, use only the first subscription for a given bookmark name.
            if (!this.bookmarkSubscriptions.ContainsKey(bookmarkTrackingQuery.Name))
            {
                this.bookmarkSubscriptions.Add(bookmarkTrackingQuery.Name, bookmarkTrackingQuery);
            }
        }

        void AddCustomTrackingSubscription(CustomTrackingQuery customQuery)
        {
            if (this.customTrackingQuerySubscriptions == null)
            {
                this.customTrackingQuerySubscriptions = new List<CustomTrackingQuery>();
            }
            this.customTrackingQuerySubscriptions.Add(customQuery);
        }

        void AddWorkflowSubscription(WorkflowInstanceQuery workflowTrackingQuery)
        {
            this.trackingRecordPreFilter.TrackWorkflowInstanceRecords = true;

            if (this.workflowEventSubscriptions == null)
            {
                this.workflowEventSubscriptions = new Dictionary<string, WorkflowInstanceQuery>();
            }
            if (workflowTrackingQuery.HasStates)
            {
                foreach (string state in workflowTrackingQuery.States)
                {
                    //if duplicates are found, use only the first subscription for a given state.
                    if (!this.workflowEventSubscriptions.ContainsKey(state))
                    {
                        this.workflowEventSubscriptions.Add(state, workflowTrackingQuery);
                    }
                }
            }
        }

        internal TrackingRecord Match(TrackingRecord record, bool shouldClone)
        {
            TrackingQuery resultQuery = null;
            if (record is WorkflowInstanceRecord)
            {
                resultQuery = Match((WorkflowInstanceRecord)record);
            }
            else if (record is ActivityStateRecord)
            {
                resultQuery = Match((ActivityStateRecord)record);
            }
            else if (record is BookmarkResumptionRecord)
            {
                resultQuery = Match((BookmarkResumptionRecord)record);
            }
            else if (record is CustomTrackingRecord)
            {
                resultQuery = Match((CustomTrackingRecord)record);
            }
            else if (record is ActivityScheduledRecord)
            {
                resultQuery = Match((ActivityScheduledRecord)record);
            }
            else if (record is CancelRequestedRecord)
            {
                resultQuery = Match((CancelRequestedRecord)record);
            }
            else if (record is FaultPropagationRecord)
            {
                resultQuery = Match((FaultPropagationRecord)record);
            }
            
            return resultQuery == null ? null : PrepareRecord(record, resultQuery, shouldClone);
        }

        ActivityStateQuery Match(ActivityStateRecord activityStateRecord)
        {
            ActivityStateQuery query = null;
            if (this.activitySubscriptions != null)
            {
                HybridCollection<ActivityStateQuery> eventSubscriptions;
                //first look for a specific match, if not found, look for a generic match.
                if (this.activitySubscriptions.TryGetValue(activityStateRecord.Activity.Name, out eventSubscriptions))
                {
                    query = MatchActivityState(activityStateRecord, eventSubscriptions.AsReadOnly());
                }

                if (query == null && this.activitySubscriptions.TryGetValue("*", out eventSubscriptions))
                {
                    query = MatchActivityState(activityStateRecord, eventSubscriptions.AsReadOnly());

                    if ((query != null) && (this.associatedProfile.ImplementationVisibility == ImplementationVisibility.RootScope))
                    {
                        if (!ShouldTrackActivity(activityStateRecord.Activity, "*"))
                        {
                            return null;
                        }
                    }
                }
            }

            return query;
        }

        static ActivityStateQuery MatchActivityState(ActivityStateRecord activityRecord, ReadOnlyCollection<ActivityStateQuery> subscriptions)
        {
            ActivityStateQuery genericMatch = null;
            for (int i = 0; i < subscriptions.Count; i++)
            {
                if (subscriptions[i].States.Contains(activityRecord.State))
                {
                    return subscriptions[i];
                }
                else if (subscriptions[i].States.Contains("*"))
                {
                    if (genericMatch == null)
                    {
                        genericMatch = subscriptions[i];
                    }
                }
            }
            return genericMatch;
        }

        WorkflowInstanceQuery Match(WorkflowInstanceRecord workflowRecord)
        {
            WorkflowInstanceQuery trackingQuery = null;
            if (this.workflowEventSubscriptions != null)
            {
                if (!this.workflowEventSubscriptions.TryGetValue(workflowRecord.State, out trackingQuery))
                {
                    this.workflowEventSubscriptions.TryGetValue("*", out trackingQuery);
                }
            }
            return trackingQuery;
        }

        BookmarkResumptionQuery Match(BookmarkResumptionRecord bookmarkRecord)
        {
            BookmarkResumptionQuery trackingQuery = null;
            if (this.bookmarkSubscriptions != null)
            {
                if (bookmarkRecord.BookmarkName != null)
                {
                    this.bookmarkSubscriptions.TryGetValue(bookmarkRecord.BookmarkName, out trackingQuery);
                }
                if (trackingQuery == null)
                {
                    this.bookmarkSubscriptions.TryGetValue("*", out trackingQuery);
                }
            }
            return trackingQuery;
        }

        ActivityScheduledQuery Match(ActivityScheduledRecord activityScheduledRecord)
        {
            ActivityScheduledQuery query = null;
            if (this.activityScheduledSubscriptions != null)
            {
                for (int i = 0; i < this.activityScheduledSubscriptions.Count; i++)
                {
                    //check specific and then generic
                    string activityName = activityScheduledRecord.Activity == null ? null : activityScheduledRecord.Activity.Name;
                    if (string.CompareOrdinal(this.activityScheduledSubscriptions[i].ActivityName, activityName) == 0)
                    {
                        if (CheckSubscription(this.activityScheduledSubscriptions[i].ChildActivityName, activityScheduledRecord.Child.Name))
                        {
                            query = this.activityScheduledSubscriptions[i];
                            break;
                        }

                    }
                    else if (string.CompareOrdinal(this.activityScheduledSubscriptions[i].ActivityName, "*") == 0)
                    {
                        if (CheckSubscription(this.activityScheduledSubscriptions[i].ChildActivityName, activityScheduledRecord.Child.Name))
                        {
                            query = this.activityScheduledSubscriptions[i];
                            break;
                        }
                    }
                }
            }

            if ((query != null) && (this.associatedProfile.ImplementationVisibility == ImplementationVisibility.RootScope))
            {
                if ((!ShouldTrackActivity(activityScheduledRecord.Activity, query.ActivityName)) ||
                        (!ShouldTrackActivity(activityScheduledRecord.Child, query.ChildActivityName)))
                {
                    return null;
                }
            }

            return query;
        }

        FaultPropagationQuery Match(FaultPropagationRecord faultRecord)
        {
            FaultPropagationQuery query = null;
            if (this.faultPropagationSubscriptions != null)
            {
                for (int i = 0; i < this.faultPropagationSubscriptions.Count; i++)
                {
                    //check specific and then generic
                    string faultHandlerName = faultRecord.FaultHandler == null ? null : faultRecord.FaultHandler.Name;
                    if (string.CompareOrdinal(this.faultPropagationSubscriptions[i].FaultSourceActivityName, faultRecord.FaultSource.Name) == 0)
                    {
                        if (CheckSubscription(this.faultPropagationSubscriptions[i].FaultHandlerActivityName, faultHandlerName))
                        {
                            query = this.faultPropagationSubscriptions[i];
                            break;
                        }
                    }
                    else if (string.CompareOrdinal(this.faultPropagationSubscriptions[i].FaultSourceActivityName, "*") == 0)
                    {
                        if (CheckSubscription(this.faultPropagationSubscriptions[i].FaultHandlerActivityName, faultHandlerName))
                        {
                            query = this.faultPropagationSubscriptions[i];
                            break;
                        }
                    }
                }
            }

            if ((query != null) && (this.associatedProfile.ImplementationVisibility == ImplementationVisibility.RootScope))
            {
                if ((!ShouldTrackActivity(faultRecord.FaultHandler, query.FaultHandlerActivityName)) ||
                    (!ShouldTrackActivity(faultRecord.FaultSource, query.FaultSourceActivityName)))
                {
                    return null;
                }
            }

            return query;
        }

        CancelRequestedQuery Match(CancelRequestedRecord cancelRecord)
        {
            CancelRequestedQuery query = null;

            if (this.cancelRequestedSubscriptions != null)
            {
                for (int i = 0; i < this.cancelRequestedSubscriptions.Count; i++)
                {
                    //check specific and then generic
                    string activityName = cancelRecord.Activity == null ? null : cancelRecord.Activity.Name;
                    if (string.CompareOrdinal(this.cancelRequestedSubscriptions[i].ActivityName, activityName) == 0)
                    {
                        if (CheckSubscription(this.cancelRequestedSubscriptions[i].ChildActivityName, cancelRecord.Child.Name))
                        {
                            query = this.cancelRequestedSubscriptions[i];
                            break;
                        }
                    }
                    else if (string.CompareOrdinal(this.cancelRequestedSubscriptions[i].ActivityName, "*") == 0)
                    {
                        if (CheckSubscription(this.cancelRequestedSubscriptions[i].ChildActivityName, cancelRecord.Child.Name))
                        {
                            query = this.cancelRequestedSubscriptions[i];
                            break;
                        }
                    }
                }
            }

            if ((query != null) && (this.associatedProfile.ImplementationVisibility == ImplementationVisibility.RootScope))
            {
                if ((!ShouldTrackActivity(cancelRecord.Activity, query.ActivityName)) ||
                    (!ShouldTrackActivity(cancelRecord.Child, query.ChildActivityName)))
                {
                    return null;
                }
            }

            return query;
        }

        CustomTrackingQuery Match(CustomTrackingRecord customRecord)
        {
            CustomTrackingQuery query = null;

            if (this.customTrackingQuerySubscriptions != null)
            {
                for (int i = 0; i < this.customTrackingQuerySubscriptions.Count; i++)
                {
                    //check specific and then generic
                    if (string.CompareOrdinal(this.customTrackingQuerySubscriptions[i].Name, customRecord.Name) == 0)
                    {
                        if (CheckSubscription(this.customTrackingQuerySubscriptions[i].ActivityName, customRecord.Activity.Name))
                        {
                            query = this.customTrackingQuerySubscriptions[i];
                            break;
                        }
                    }
                    else if (string.CompareOrdinal(this.customTrackingQuerySubscriptions[i].Name, "*") == 0)
                    {
                        if (CheckSubscription(this.customTrackingQuerySubscriptions[i].ActivityName, customRecord.Activity.Name))
                        {
                            query = this.customTrackingQuerySubscriptions[i];
                            break;
                        }
                    }
                }
            }
            return query;
        }

        static bool CheckSubscription(string name, string value)
        {
            //check specific and then generic
            return (string.CompareOrdinal(name, value) == 0 ||
                string.CompareOrdinal(name, "*") == 0);
        }

        static void ExtractVariables(ActivityStateRecord activityStateRecord, ActivityStateQuery activityStateQuery)
        {
            if (activityStateQuery.HasVariables)
            {
                activityStateRecord.Variables = activityStateRecord.GetVariables(activityStateQuery.Variables);
            }
            else
            {
                activityStateRecord.Variables = ActivityUtilities.EmptyParameters;
            }
        }

        static void ExtractArguments(ActivityStateRecord activityStateRecord, ActivityStateQuery activityStateQuery)
        {
            if (activityStateQuery.HasArguments)
            {
                activityStateRecord.Arguments = activityStateRecord.GetArguments(activityStateQuery.Arguments);
            }
            else
            {
                activityStateRecord.Arguments = ActivityUtilities.EmptyParameters;
            }
        }

        static TrackingRecord PrepareRecord(TrackingRecord record, TrackingQuery query, bool shouldClone)
        {
            TrackingRecord preparedRecord = shouldClone ? record.Clone() : record;

            if (query.HasAnnotations)
            {
                preparedRecord.Annotations = new ReadOnlyDictionaryInternal<string, string>(query.QueryAnnotations);
            }

            if (query is ActivityStateQuery)
            {
                ExtractArguments((ActivityStateRecord)preparedRecord, (ActivityStateQuery)query);
                ExtractVariables((ActivityStateRecord)preparedRecord, (ActivityStateQuery)query);                
            }
            return preparedRecord;
        }


        class RuntimeTrackingProfileCache
        {
            [Fx.Tag.Cache(typeof(RuntimeTrackingProfile), Fx.Tag.CacheAttrition.PartialPurgeOnEachAccess)]
            ConditionalWeakTable<Activity, HybridCollection<RuntimeTrackingProfile>> cache;

            public RuntimeTrackingProfileCache()
            {
                this.cache = new ConditionalWeakTable<Activity, HybridCollection<RuntimeTrackingProfile>>();
            }

            public RuntimeTrackingProfile GetRuntimeTrackingProfile(TrackingProfile profile, Activity rootElement)
            {
                Fx.Assert(rootElement != null, "Root element must be valid");

                RuntimeTrackingProfile foundRuntimeProfile = null;
                HybridCollection<RuntimeTrackingProfile> runtimeProfileList = null;

                lock (this.cache)
                {
                    if (!this.cache.TryGetValue(rootElement, out runtimeProfileList))
                    {
                        foundRuntimeProfile = new RuntimeTrackingProfile(profile, rootElement);
                        runtimeProfileList = new HybridCollection<RuntimeTrackingProfile>();
                        runtimeProfileList.Add(foundRuntimeProfile);

                        this.cache.Add(rootElement, runtimeProfileList);
                    }
                    else
                    {
                        ReadOnlyCollection<RuntimeTrackingProfile> runtimeProfileCollection = runtimeProfileList.AsReadOnly();
                        foreach (RuntimeTrackingProfile runtimeProfile in runtimeProfileCollection)
                        {
                            if (string.CompareOrdinal(profile.Name, runtimeProfile.associatedProfile.Name) == 0 &&
                                string.CompareOrdinal(profile.ActivityDefinitionId, runtimeProfile.associatedProfile.ActivityDefinitionId) == 0)
                            {
                                foundRuntimeProfile = runtimeProfile;
                                break;
                            }
                        }

                        if (foundRuntimeProfile == null)
                        {
                            foundRuntimeProfile = new RuntimeTrackingProfile(profile, rootElement);
                            runtimeProfileList.Add(foundRuntimeProfile);
                        }
                    }
                }
                return foundRuntimeProfile;
            }
        }
    }
}
