
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Globalization;

//using System.Workflow.Activities;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime;
using System.Workflow.Runtime.Hosting;
using Hosting = System.Workflow.Runtime.Hosting;
using System.Workflow.Runtime.Tracking;


namespace System.Workflow.Runtime
{
    /// <summary>
    /// RTTrackingProfile contains functionality specific to the runtime such as 
    /// trackpoint and location matching and caching, cloning, handling dynamic updates...
    /// </summary>
    internal class RTTrackingProfile : ICloneable // ICloneable is deprecated
    {
        #region Private Data Members
        //
        // Client defined profile
        private TrackingProfile _profile = null;
        //
        // Type of the workflow that this profile is associated to
        private Type _workflowType = null;
        private Type _serviceType = null;
        //
        // List of qualified ids and the trackpoints that declared themselves as matches during static examination
        private Dictionary<string, List<ActivityTrackPointCacheItem>> _activities = new Dictionary<string, List<ActivityTrackPointCacheItem>>();
        private List<string> _activitiesIgnore = new List<string>();

        private Dictionary<string, List<UserTrackPoint>> _user = new Dictionary<string, List<UserTrackPoint>>();
        private List<string> _userIgnore = new List<string>();
        //
        // Indicates that the RTTrackingProfile instance is private and is safe to modify for a specific instance
        private bool _isPrivate = false;
        //
        // Indicates if a dynamic update is in-flight
        private bool _pendingWorkflowChange = false;
        //
        // The changes for a dynamic update
        private IList<WorkflowChangeAction> _pendingChanges = null;
        //
        // Activities (including those that are being added) can start executing while a dynamic update is pending
        // These cannot be added to the main cache until the update succeeds because the update might roll back.
        // However since we have to search for matching track points we might as well save that work. 
        // This list will be copied into the main cache if the dynamic update completes successfully
        private Dictionary<string, List<ActivityTrackPointCacheItem>> _dynamicActivities = null;
        private List<string> _dynamicActivitiesIgnore = null;

        private Dictionary<string, List<UserTrackPoint>> _dynamicUser = null;
        private List<string> _dynamicUserIgnore = null;

        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        protected RTTrackingProfile()
        {
        }
        /// <summary>
        /// Primary constructor
        /// </summary>
        /// <param name="root"></param>
        /// <param name="workflowType"></param>
        /// <param name="profile"></param>
        internal RTTrackingProfile(TrackingProfile profile, Activity root, Type serviceType)
        {
            if (null == profile)
                throw new ArgumentNullException("profile");
            if (null == root)
                throw new ArgumentNullException("root");
            if (null == serviceType)
                throw new ArgumentNullException("serviceType");

            _workflowType = root.GetType();
            _serviceType = serviceType;
            //
            // "Clone" a private copy in case the tracking service holds a reference to 
            // the profile it gave us and attempts to modify it at a later point
            TrackingProfileSerializer tps = new TrackingProfileSerializer();

            StringWriter writer = new StringWriter(System.Globalization.CultureInfo.InvariantCulture);
            StringReader reader = null;

            TrackingProfile privateProfile = null;

            try
            {
                //
                // Let exceptions bubble back to the tracking service - 
                // the profile must be valid per the schema.
                tps.Serialize(writer, profile);
                reader = new StringReader(writer.ToString());
                privateProfile = tps.Deserialize(reader);
            }
            finally
            {
                if (null != reader)
                    reader.Close();

                if (null != writer)
                    writer.Close();
            }
            _profile = privateProfile;

            CheckAllActivities((Activity)root);
        }

        /// <summary>
        /// Constructor used for cloning.  
        /// </summary>
        /// <param name="runtimeProfile">RTTrackingProfile to clone</param>
        /// <remarks>All members are shallow copied!  Use MakePrivate to deep copy after cloning.</remarks>
        private RTTrackingProfile(RTTrackingProfile runtimeProfile)
        {
            //
            // Shallow copy
            _profile = runtimeProfile._profile;
            _isPrivate = runtimeProfile._isPrivate;
            _pendingChanges = runtimeProfile._pendingChanges;
            _pendingWorkflowChange = runtimeProfile._pendingWorkflowChange;
            _workflowType = runtimeProfile._workflowType;
            //
            // Deep copy the cache.  Items in the cache can
            // be shared but the cache themselves cannot as they may be modified
            //
            // Activity match and ignore cache
            _activities = new Dictionary<string, List<ActivityTrackPointCacheItem>>(runtimeProfile._activities.Count);
            foreach (KeyValuePair<string, List<ActivityTrackPointCacheItem>> kvp in runtimeProfile._activities)
                _activities.Add(kvp.Key, runtimeProfile._activities[kvp.Key]);

            _activitiesIgnore = new List<string>(runtimeProfile._activitiesIgnore);
            //
            // Pending dynamic update activity match and ignore cache
            if (null != runtimeProfile._dynamicActivities)
            {
                _dynamicActivities = new Dictionary<string, List<ActivityTrackPointCacheItem>>(runtimeProfile._dynamicActivities.Count);
                foreach (KeyValuePair<string, List<ActivityTrackPointCacheItem>> kvp in runtimeProfile._dynamicActivities)
                    _dynamicActivities.Add(kvp.Key, runtimeProfile._dynamicActivities[kvp.Key]);
            }

            if (null != runtimeProfile._dynamicActivitiesIgnore)
                _dynamicActivitiesIgnore = new List<string>(runtimeProfile._dynamicActivitiesIgnore);
            //
            // User event match and ignore cache
            _user = new Dictionary<string, List<UserTrackPoint>>(runtimeProfile._user.Count);
            foreach (KeyValuePair<string, List<UserTrackPoint>> kvp in runtimeProfile._user)
                _user.Add(kvp.Key, runtimeProfile._user[kvp.Key]);

            _userIgnore = new List<string>(runtimeProfile._userIgnore);
            //
            // Pending dynamic update activity match and ignore cache
            if (null != runtimeProfile._dynamicUser)
            {
                _dynamicUser = new Dictionary<string, List<UserTrackPoint>>(runtimeProfile._dynamicUser.Count);
                foreach (KeyValuePair<string, List<UserTrackPoint>> kvp in runtimeProfile._dynamicUser)
                    _dynamicUser.Add(kvp.Key, kvp.Value);
            }

            if (null != runtimeProfile._dynamicUserIgnore)
                _dynamicUserIgnore = new List<string>(runtimeProfile._dynamicUserIgnore);
        }

        #endregion

        #region Properties
        /// <summary>
        /// Indicates if the profile is specific to an individual instance.
        /// </summary>
        internal bool IsPrivate
        {
            get { return _isPrivate; }
            set
            {
                if (!(value) && (_isPrivate))
                    throw new InvalidOperationException(ExecutionStringManager.CannotResetIsPrivate);

                _isPrivate = value;
            }
        }
        /// <summary>
        /// Type of workflow to which this profile is associated
        /// </summary>
        internal Type WorkflowType
        {
            get { return _workflowType; }
        }

        /// <summary>
        /// Version of the profile
        /// </summary>
        internal Version Version
        {
            get { return _profile.Version; }
        }

        #endregion

        #region Internal Methods for Listeners

        internal bool TryTrackActivityEvent(Activity activity, ActivityExecutionStatus status, IServiceProvider provider, ActivityTrackingRecord record)
        {
            List<ActivityTrackPointCacheItem> points;
            //
            // Check the match caches.
            if (TryGetCacheItems(activity, out points))
            {
                bool ret = false;
                foreach (ActivityTrackPointCacheItem item in points)
                {
                    if (item.HasLocationConditions)
                    {
                        if (!item.Point.IsMatch(activity, status))
                            continue;
                    }

                    if (item.Events.Contains(status))
                    {
                        ret = true;
                        item.Point.Track(activity, provider, record.Body);
                        record.Annotations.AddRange(item.Point.Annotations);
                    }
                }
                return ret;
            }
            return false;
        }

        internal bool TryTrackUserEvent(Activity activity, string keyName, object argument, WorkflowExecutor exec, UserTrackingRecord record)
        {
            List<UserTrackPoint> points;
            if (TryGetCacheItems(activity, out points))
            {
                bool ret = false;
                foreach (UserTrackPoint point in points)
                {
                    if (point.IsMatch(activity, keyName, argument))
                    {
                        ret = true;
                        point.Track(activity, argument, exec, record.Body);
                        record.Annotations.AddRange(point.Annotations);
                    }
                }
                return ret;
            }
            return false;
        }

        internal bool TryTrackInstanceEvent(TrackingWorkflowEvent status, WorkflowTrackingRecord record)
        {
            bool track = false;
            foreach (WorkflowTrackPoint point in _profile.WorkflowTrackPoints)
            {
                if (point.IsMatch(status))
                {
                    record.Annotations.AddRange(point.Annotations);
                    track = true;
                }
            }
            return track;
        }


        /// <summary>
        /// Called by TrackingListener to determine if a subscription is needed for an activity.
        /// Also used as an entry point for dynamically building cache entries for dynamically added activities.  
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="exec"></param>
        /// <returns></returns>
        internal bool ActivitySubscriptionNeeded(Activity activity)
        {
            List<ActivityTrackPointCacheItem> points = null;
            if ((!_pendingWorkflowChange) || ((_pendingWorkflowChange) && (!IsPendingUpdateActivity(activity, true))))
            {
                //
                // A dynamic update is not in progress or 
                // the activity is not part of the dynamic update.
                // The main cache has all matching track points
                //
                // 
                bool retry = true;
                while (retry)
                {
                    if (_activitiesIgnore.Contains(activity.QualifiedName))
                        return false;

                    if (_activities.TryGetValue(activity.QualifiedName, out points))
                        return true;
                    else
                        //
                        // This activity isn't in either cache, look it up in the profile and add to cache
                        CheckActivity(activity);
                }
                return false;
            }
            else
            {
                //
                // Dynamic update is in progress and this activity is being added as part of the update
                // Search the profile for matching track points and add them to the dynamic cache
                // (copied to the main cache at the successful completion of the update)
                // Don't go through CheckActivity because that adds to the main cache
                List<UserTrackPoint> user = null;
                if (CreateCacheItems(activity, out user))
                    CacheInsertUpdatePending(activity.QualifiedName, user);
                else
                    _dynamicUserIgnore.Add(activity.QualifiedName);

                if (CreateCacheItems(activity, out points))
                {
                    CacheInsertUpdatePending(activity.QualifiedName, points);
                    return true;
                }
                else
                {
                    _dynamicActivitiesIgnore.Add(activity.QualifiedName);
                    return false;
                }
            }
        }

        public void WorkflowChangeBegin(IList<WorkflowChangeAction> changeActions)
        {
            Debug.Assert(!_pendingWorkflowChange, "_pendingWorkflowChange should be false.");
            if (_pendingWorkflowChange)
                throw new InvalidOperationException(ExecutionStringManager.DynamicUpdateIsNotPending);

            if (!_isPrivate)
                throw new InvalidOperationException(ExecutionStringManager.ProfileIsNotPrivate);
            //
            // Initialize the temp dictionary for activities that are spun up during the update process
            // If the update succeeds we'll copy these to the main _subscriptions dictionary.
            _dynamicActivities = new Dictionary<string, List<ActivityTrackPointCacheItem>>();
            _dynamicActivitiesIgnore = new List<string>();

            _dynamicUser = new Dictionary<string, List<UserTrackPoint>>();
            _dynamicUserIgnore = new List<string>();

            _pendingChanges = changeActions;
            _pendingWorkflowChange = true;
        }

        public void WorkflowChangeCommit()
        {
            Debug.Assert(_pendingWorkflowChange, "Workflow change is not pending - no change to commit");

            if (!_pendingWorkflowChange)
                return;

            if (!_isPrivate)
                throw new InvalidOperationException(ExecutionStringManager.ProfileIsNotPrivate);
            //
            // Remove items that have been deleted by this update
            // Must do all removes first as there may be a new action 
            // with the same qid as a previous action that is being removed
            if (null != _pendingChanges)
            {
                foreach (WorkflowChangeAction action in _pendingChanges)
                {
                    if (action is RemovedActivityAction)
                    {
                        //
                        // Remove all references to this activity that might exist in our caches
                        string qId = ((RemovedActivityAction)action).OriginalRemovedActivity.QualifiedName;
                        _activities.Remove(qId);
                        _activitiesIgnore.Remove(qId);
                        _user.Remove(qId);
                        _userIgnore.Remove(qId);
                    }
                }
            }
            //
            // Copy any pending cache items to the regular activity track point cache
            if ((null != _dynamicActivities) && (_dynamicActivities.Count > 0))
                foreach (KeyValuePair<string, List<ActivityTrackPointCacheItem>> kvp in _dynamicActivities)
                    _activities.Add(kvp.Key, kvp.Value);

            if ((null != _dynamicActivitiesIgnore) && (_dynamicActivitiesIgnore.Count > 0))
                _activitiesIgnore.AddRange(_dynamicActivitiesIgnore);

            if ((null != _dynamicUser) && (_dynamicUser.Count > 0))
                foreach (KeyValuePair<string, List<UserTrackPoint>> kvp in _dynamicUser)
                    _user.Add(kvp.Key, kvp.Value);

            if ((null != _dynamicUserIgnore) && (_dynamicUserIgnore.Count > 0))
                _userIgnore.AddRange(_dynamicUserIgnore);
            //
            // All done, clean up
            _dynamicActivities = null;
            _dynamicActivitiesIgnore = null;
            _dynamicUser = null;
            _dynamicUserIgnore = null;
            _pendingChanges = null;
            _pendingWorkflowChange = false;
        }

        public void WorkflowChangeRollback()
        {
            //
            // Just clean up, there isn't any work to rollback because
            // any subscriptions that may have been added for a pending add activity
            // won't ever be hit as the activities haven't been added to the tree.
            _dynamicActivities = null;
            _dynamicActivitiesIgnore = null;
            _dynamicUser = null;
            _dynamicUserIgnore = null;
            _pendingChanges = null;
            _pendingWorkflowChange = false;
        }

        #endregion

        #region Private Cache Methods

        /// <summary>
        /// Create the static qualifiedid to trackpoint map
        /// </summary>
        /// <param name="root"></param>
        private void CheckAllActivities(Activity activity)
        {
            CheckActivity((Activity)activity);
            //
            // Walk down the activity tree
            // Use EnabledActivities to get invisible activities
            // EnabledActivities will not return commented activities
            if (activity is CompositeActivity)
                foreach (Activity a in GetAllEnabledActivities((CompositeActivity)activity))
                    CheckAllActivities(a);
        }

        /// <summary>
        /// Recursively walk the activity tree and find all track points that match each activity
        /// </summary>
        /// <param name="activity"></param>
        private void CheckActivity(Activity activity)
        {
            //
            // Build caches of activity status change events
            string qId = activity.QualifiedName;
            List<ActivityTrackPointCacheItem> activities = null;
            if (CreateCacheItems(activity, out activities))
                CacheInsert(qId, activities);
            else
                _activitiesIgnore.Add(qId);

            //
            // Build caches of user events
            List<UserTrackPoint> user = null;
            if (CreateCacheItems(activity, out user))
                CacheInsert(qId, user);
            else
                _userIgnore.Add(qId);
        }

        /// <summary>
        /// Find all trackpoints that match an activity.
        /// </summary>
        /// <param name="activity">Activity for which to determine subscription needs</param>
        /// <param name="includes">List to be populated with matching track points</param>
        /// <returns>true if a subscription is needed; false if not</returns>
        private bool CreateCacheItems(Activity activity, out List<ActivityTrackPointCacheItem> includes)
        {
            includes = new List<ActivityTrackPointCacheItem>();
            //
            // Check if we have any trackpoints that match this activity
            foreach (ActivityTrackPoint point in _profile.ActivityTrackPoints)
            {
                List<ActivityExecutionStatus> events;
                bool hasCondition = false;
                if (point.IsMatch(activity, out events, out hasCondition))
                    includes.Add(new ActivityTrackPointCacheItem(point, events, hasCondition));
            }

            return (includes.Count > 0);
        }

        /// <summary>
        /// Find all trackpoints that match user events for an activity.
        /// </summary>
        /// <param name="activity">Activity for which to determine subscription needs</param>
        /// <param name="includes">List to be populated with matching track points</param>
        /// <returns>true if a subscription is needed; false if not</returns>
        private bool CreateCacheItems(Activity activity, out List<UserTrackPoint> includes)
        {
            includes = new List<UserTrackPoint>();
            //
            // Check if we have any trackpoints that match this activity
            foreach (UserTrackPoint point in _profile.UserTrackPoints)
            {
                if (point.IsMatch(activity))
                    includes.Add(point);
            }

            return (includes.Count > 0);
        }

        private void CacheInsert(string qualifiedID, List<ActivityTrackPointCacheItem> points)
        {
            //
            // Check to make sure the item isn't in the dictionary
            // If not add all track points
            Debug.Assert(!_activities.ContainsKey(qualifiedID), "QualifiedName is already in the activities cache");
            if (_activities.ContainsKey(qualifiedID))
                throw new InvalidOperationException(ExecutionStringManager.RTProfileActCacheDupKey);

            foreach (ActivityTrackPointCacheItem point in points)
                CacheInsert(qualifiedID, point);
        }

        private void CacheInsert(string qualifiedID, List<UserTrackPoint> points)
        {
            //
            // Check to make sure the item isn't in the dictionary
            // If not add all track points
            Debug.Assert(!_user.ContainsKey(qualifiedID), "QualifiedName is already in the user cache");
            if (_user.ContainsKey(qualifiedID))
                throw new InvalidOperationException(ExecutionStringManager.RTProfileActCacheDupKey);

            foreach (UserTrackPoint point in points)
                CacheInsert(qualifiedID, point);
        }

        private void CacheInsert(string qualifiedID, ActivityTrackPointCacheItem point)
        {
            List<ActivityTrackPointCacheItem> points = null;

            if (!_activities.TryGetValue(qualifiedID, out points))
            {
                points = new List<ActivityTrackPointCacheItem>();
                _activities.Add(qualifiedID, points);
            }
            points.Add(point);
        }

        private void CacheInsert(string qualifiedID, UserTrackPoint point)
        {
            List<UserTrackPoint> points = null;

            if (!_user.TryGetValue(qualifiedID, out points))
            {
                points = new List<UserTrackPoint>();
                _user.Add(qualifiedID, points);
            }
            points.Add(point);
        }

        private void CacheInsertUpdatePending(string qualifiedID, List<ActivityTrackPointCacheItem> points)
        {
            //
            // The activity has been added during a pending dynamic change
            // add it to a temporary lookup which will be copied to real cache 
            // when the dynamic update commits.
            if ((!_isPrivate) || (!_pendingWorkflowChange))
                throw new InvalidOperationException(ExecutionStringManager.ProfileIsNotPrivate);

            if (null == _dynamicActivities)
                throw new InvalidOperationException(ExecutionStringManager.RTProfileDynamicActCacheIsNull);

            List<ActivityTrackPointCacheItem> tmp = null;
            if (!_dynamicActivities.TryGetValue(qualifiedID, out tmp))
            {
                tmp = new List<ActivityTrackPointCacheItem>();
                _dynamicActivities.Add(qualifiedID, tmp);
            }

            foreach (ActivityTrackPointCacheItem point in points)
                tmp.Add(point);
        }

        private bool TryGetCacheItems(Activity activity, out List<ActivityTrackPointCacheItem> points)
        {
            points = null;
            if ((!_pendingWorkflowChange) || ((_pendingWorkflowChange) && (!IsPendingUpdateActivity(activity, true))))
            {
                //
                // A dynamic update is not in progress or this activity 
                // is not being added by the current dynamic update.
                // The main cache holds all matching track points
                return _activities.TryGetValue(activity.QualifiedName, out points);
            }
            else
            {
                //
                // Dynamic update is in progress
                return _dynamicActivities.TryGetValue(activity.QualifiedName, out points);
            }
        }

        private void CacheInsertUpdatePending(string qualifiedID, List<UserTrackPoint> points)
        {
            //
            // The activity has been added during a pending dynamic change
            // add it to a temporary lookup which will be copied to real cache 
            // when the dynamic update commits.
            if ((!_isPrivate) || (!_pendingWorkflowChange))
                throw new InvalidOperationException(ExecutionStringManager.ProfileIsNotPrivate);

            if (null == _dynamicUser)
                throw new InvalidOperationException(ExecutionStringManager.RTProfileDynamicActCacheIsNull);

            List<UserTrackPoint> tmp = null;
            if (!_dynamicUser.TryGetValue(qualifiedID, out tmp))
            {
                tmp = new List<UserTrackPoint>();
                _dynamicUser.Add(qualifiedID, tmp);
            }

            foreach (UserTrackPoint point in points)
                tmp.Add(point);
        }

        private bool TryGetCacheItems(Activity activity, out List<UserTrackPoint> points)
        {
            points = null;
            if ((!_pendingWorkflowChange) || ((_pendingWorkflowChange) && (!IsPendingUpdateActivity(activity, true))))
            {
                //
                // A dynamic update is not in progress or this activity 
                // is not being added by the current dynamic update.
                // The main cache holds all matching track points
                return _user.TryGetValue(activity.QualifiedName, out points);
            }
            else
            {
                //
                // Dynamic update is in progress
                return _dynamicUser.TryGetValue(activity.QualifiedName, out points);
            }
        }
        #endregion

        #region Private Methods


        // This function returns all the executable activities including secondary flow activities.
        public IList<Activity> GetAllEnabledActivities(CompositeActivity compositeActivity)
        {
            if (compositeActivity == null)
                throw new ArgumentNullException("compositeActivity");

            List<Activity> allActivities = new List<Activity>(compositeActivity.EnabledActivities);

            foreach (Activity secondaryFlowActivity in ((ISupportAlternateFlow)compositeActivity).AlternateFlowActivities)
            {
                if (!allActivities.Contains(secondaryFlowActivity))
                    allActivities.Add(secondaryFlowActivity);
            }

            return allActivities;
        }

        private bool IsPendingUpdateActivity(Activity activity, bool addedOnly)
        {
            //
            // If we don't have an update going on this method isn't valid
            if ((!_isPrivate) || (!_pendingWorkflowChange))
                throw new InvalidOperationException(ExecutionStringManager.ProfileIsNotPrivate);
            //
            // if we don't have any changes we're done
            if ((null == _pendingChanges || _pendingChanges.Count <= 0))
                return false;

            foreach (WorkflowChangeAction action in _pendingChanges)
            {
                string qualifiedId = null;
                if (action is ActivityChangeAction)
                {
                    if (action is AddedActivityAction)
                    {
                        qualifiedId = ((AddedActivityAction)action).AddedActivity.QualifiedName;
                    }
                    else if (action is RemovedActivityAction)
                    {
                        if (!addedOnly)
                            qualifiedId = ((RemovedActivityAction)action).OriginalRemovedActivity.QualifiedName;
                    }
                    else
                    {
                        Debug.Assert(false, ExecutionStringManager.UnknownActivityActionType);
                    }

                    if ((null != qualifiedId)
                        && (0 == String.Compare(activity.QualifiedName, qualifiedId, StringComparison.Ordinal)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion

        #region ICloneable Members

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        internal RTTrackingProfile Clone()
        {
            return new RTTrackingProfile(this);
        }

        #endregion

        #region Contained Types

        private struct ActivityTrackPointCacheItem
        {
            internal ActivityTrackPointCacheItem(ActivityTrackPoint point, List<ActivityExecutionStatus> events, bool hasConditions)
            {
                if (null == point)
                    throw new ArgumentNullException("point");
                if (null == events)
                    throw new ArgumentNullException("events");

                Point = point;
                Events = events;
                HasLocationConditions = hasConditions;
            }

            internal ActivityTrackPoint Point;
            internal List<ActivityExecutionStatus> Events;
            internal bool HasLocationConditions;
        }

        #endregion
    }
}
