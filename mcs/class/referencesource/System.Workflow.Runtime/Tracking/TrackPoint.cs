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

namespace System.Workflow.Runtime.Tracking
{

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class ActivityTrackPoint
    {
        #region Private Data Members

        private ActivityTrackingLocationCollection _match = new ActivityTrackingLocationCollection();
        private ActivityTrackingLocationCollection _exclude = new ActivityTrackingLocationCollection();
        private TrackingAnnotationCollection _annotations = new TrackingAnnotationCollection();
        private ExtractCollection _extracts = new ExtractCollection();

        #endregion

        #region Public Properties

        public ActivityTrackingLocationCollection MatchingLocations
        {
            get { return _match; }
        }

        public ActivityTrackingLocationCollection ExcludedLocations
        {
            get { return _exclude; }
        }

        public TrackingAnnotationCollection Annotations
        {
            get { return _annotations; }
        }

        public ExtractCollection Extracts
        {
            get { return _extracts; }
        }

        #endregion

        #region Internal Matching Methods

        internal bool IsMatch(Activity activity, out List<ActivityExecutionStatus> status, out bool hasCondition)
        {
            hasCondition = false;
            //
            // Check if we have any conditions on this track point.
            // If we do signal that we need to recheck this item for each activity event (can't cache)
            foreach (ActivityTrackingLocation location in _exclude)
            {
                if ((null != location.Conditions) && (location.Conditions.Count > 0))
                {
                    hasCondition = true;
                    break;
                }
            }
            foreach (ActivityTrackingLocation location in _match)
            {
                if ((null != location.Conditions) && (location.Conditions.Count > 0))
                {
                    hasCondition = true;
                    break;
                }
            }
            status = new List<ActivityExecutionStatus>(9);
            //
            // Do matches first
            foreach (ActivityTrackingLocation location in _match)
            {
                if (location.Match(activity, true))
                {
                    //
                    // Insert all status values for this location
                    foreach (ActivityExecutionStatus s in location.ExecutionStatusEvents)
                    {
                        if (!status.Contains(s))
                            status.Add(s);
                    }
                }
            }
            //
            // If no includes matched 
            // this trackpoint isn't relevant to this activity
            if (0 == status.Count)
                return false;
            //
            // Check the excludes but only if there aren't any conditions
            if (!hasCondition)
            {
                foreach (ActivityTrackingLocation location in _exclude)
                {
                    if (location.Match(activity, true))
                    {
                        //
                        // Remove all status values for this location
                        foreach (ActivityExecutionStatus s in location.ExecutionStatusEvents)
                            status.Remove(s);
                    }
                }
            }

            return (status.Count > 0);
        }

        internal bool IsMatch(Activity activity, ActivityExecutionStatus status)
        {
            //
            // Do matches first
            bool included = false;
            foreach (ActivityTrackingLocation location in _match)
            {
                if (location.Match(activity, false))
                {
                    if (location.ExecutionStatusEvents.Contains(status))
                    {
                        included = true;
                        break;
                    }
                }
            }
            //
            // If no includes matched this trackpoint 
            // doesn't match this activity
            if (!included)
                return false;
            //
            // Check the excludes
            foreach (ActivityTrackingLocation location in _exclude)
            {
                //
                // If any exclude matches this trackpoint
                // doesn't match this activity
                if (location.Match(activity, false))
                {
                    if (location.ExecutionStatusEvents.Contains(status))
                        return false;
                }
            }

            return included;
        }

        internal void Track(Activity activity, IServiceProvider provider, IList<TrackingDataItem> items)
        {
            foreach (TrackingExtract e in _extracts)
                e.GetData(activity, provider, items);
        }

        #endregion

    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class UserTrackPoint
    {
        #region Private Data Members

        private UserTrackingLocationCollection _match = new UserTrackingLocationCollection();
        private UserTrackingLocationCollection _exclude = new UserTrackingLocationCollection();
        private TrackingAnnotationCollection _annotations = new TrackingAnnotationCollection();
        private ExtractCollection _extracts = new ExtractCollection();

        #endregion

        #region Public Properties

        public UserTrackingLocationCollection MatchingLocations
        {
            get { return _match; }
        }

        public UserTrackingLocationCollection ExcludedLocations
        {
            get { return _exclude; }
        }

        public TrackingAnnotationCollection Annotations
        {
            get { return _annotations; }
        }

        public ExtractCollection Extracts
        {
            get { return _extracts; }
        }

        #endregion

        #region Internal Matching Methods

        internal bool IsMatch(Activity activity)
        {
            //
            // Check include, excludes checked at event time
            foreach (UserTrackingLocation location in _match)
                if (location.Match(activity))
                    return true;

            return false;
        }

        internal bool IsMatch(Activity activity, string keyName, object argument)
        {
            //
            // We need to check runtime values here
            //
            // Check the excludes - if any exclude matches based on activity, key and arg type we're not a match
            foreach (UserTrackingLocation location in _exclude)
                if (location.Match(activity, keyName, argument))
                    return false;
            //
            // No excludes match, check includes
            foreach (UserTrackingLocation location in _match)
                if (location.Match(activity, keyName, argument))
                    return true;

            return false;
        }

        internal void Track(Activity activity, object arg, IServiceProvider provider, IList<TrackingDataItem> items)
        {
            foreach (TrackingExtract e in _extracts)
                e.GetData(activity, provider, items);
        }

        #endregion
    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class WorkflowTrackPoint
    {
        #region Private Data Members

        private WorkflowTrackingLocation _location = new WorkflowTrackingLocation();
        private TrackingAnnotationCollection _annotations = new TrackingAnnotationCollection();

        #endregion

        #region Public Properties

        public WorkflowTrackingLocation MatchingLocation
        {
            get { return _location; }
            set { _location = value; }
        }

        public TrackingAnnotationCollection Annotations
        {
            get { return _annotations; }
        }

        #endregion

        #region Internal Matching Methods

        internal bool IsMatch(TrackingWorkflowEvent status)
        {
            return _location.Match(status);
        }

        #endregion
    }
}
