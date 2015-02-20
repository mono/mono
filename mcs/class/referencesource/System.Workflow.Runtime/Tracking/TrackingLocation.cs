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
    /// <summary>
    /// Contains data that is used to match instance locations.
    /// </summary>
    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class WorkflowTrackingLocation
    {
        #region Private Data Members

        private IList<TrackingWorkflowEvent> _events = new List<TrackingWorkflowEvent>();

        #endregion

        #region Constructors

        public WorkflowTrackingLocation()
        {
        }

        public WorkflowTrackingLocation(IList<TrackingWorkflowEvent> events)
        {
            _events = events;
        }
        #endregion

        #region Public Properties

        public IList<TrackingWorkflowEvent> Events
        {
            get { return _events; }
        }

        #endregion

        #region Internal Matching Methods

        internal bool Match(TrackingWorkflowEvent status)
        {
            return _events.Contains(status);
        }

        #endregion
    }

    /// <summary>
    /// Contains data that is used to match activity locations.
    /// </summary>
    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class ActivityTrackingLocation
    {
        #region Private Data Members

        private TrackingConditionCollection _conditions = new TrackingConditionCollection();
        private List<ActivityExecutionStatus> _events = new List<ActivityExecutionStatus>();
        private Type _activityType = null;
        private string _activityName = null;
        private bool _trackDerived = false;

        #endregion

        #region Constructors

        public ActivityTrackingLocation()
        {
        }

        public ActivityTrackingLocation(string activityTypeName)
        {
            if (null == activityTypeName)
                throw new ArgumentNullException("activityTypeName");

            _activityName = activityTypeName;
        }

        public ActivityTrackingLocation(Type activityType)
        {
            if (null == activityType)
                throw new ArgumentNullException("activityType");

            _activityType = activityType;
        }

        public ActivityTrackingLocation(string activityTypeName, IEnumerable<ActivityExecutionStatus> executionStatusEvents)
        {
            if (null == activityTypeName)
                throw new ArgumentNullException("activityTypeName");

            if (null == executionStatusEvents)
                throw new ArgumentNullException("executionStatusEvents");

            _activityName = activityTypeName;
            _events.AddRange(executionStatusEvents);
        }

        public ActivityTrackingLocation(Type activityType, IEnumerable<ActivityExecutionStatus> executionStatusEvents)
        {
            if (null == activityType)
                throw new ArgumentNullException("activityType");

            if (null == executionStatusEvents)
                throw new ArgumentNullException("executionStatusEvents");

            _activityType = activityType;
            _events.AddRange(executionStatusEvents);
        }

        public ActivityTrackingLocation(string activityTypeName, bool matchDerivedTypes, IEnumerable<ActivityExecutionStatus> executionStatusEvents)
        {
            if (null == activityTypeName)
                throw new ArgumentNullException("activityTypeName");

            if (null == executionStatusEvents)
                throw new ArgumentNullException("executionStatusEvents");

            _activityName = activityTypeName;
            _trackDerived = matchDerivedTypes;
            _events.AddRange(executionStatusEvents);
        }

        public ActivityTrackingLocation(Type activityType, bool matchDerivedTypes, IEnumerable<ActivityExecutionStatus> executionStatusEvents)
        {
            if (null == activityType)
                throw new ArgumentNullException("activityType");

            if (null == executionStatusEvents)
                throw new ArgumentNullException("executionStatusEvents");

            _activityType = activityType;
            _trackDerived = matchDerivedTypes;
            _events.AddRange(executionStatusEvents);
        }

        #endregion

        #region Public Properties

        public Type ActivityType
        {
            get { return _activityType; }
            set { _activityType = value; }
        }

        public string ActivityTypeName
        {
            get { return _activityName; }
            set { _activityName = value; }
        }

        public bool MatchDerivedTypes
        {
            get { return _trackDerived; }
            set { _trackDerived = value; }
        }

        public IList<ActivityExecutionStatus> ExecutionStatusEvents
        {
            get { return _events; }
        }

        public TrackingConditionCollection Conditions
        {
            get { return _conditions; }
        }

        #endregion

        #region Internal Matching Methods

        internal bool Match(Activity activity, bool typeMatchOnly)
        {
            if (null == activity)
                throw new ArgumentNullException("activity");
            //
            // Matching the type is generally going to be cheaper 
            // so do it first and short circuit if we don't match
            if (!TypeIsMatch(activity))
            {
                return false;
            }
            else
            {
                if (typeMatchOnly)
                {
                    return true;
                }
                else
                {
                    return ConditionsAreMatch(activity);
                }
            }
        }

        #endregion

        #region Private Matching Methods

        private bool TypeIsMatch(Activity activity)
        {
            if (null != _activityType)
                return TypeMatch.IsMatch(activity, _activityType, _trackDerived);
            else
                return TypeMatch.IsMatch(activity, _activityName, _trackDerived);
        }

        private bool ConditionsAreMatch(object obj)
        {
            //
            // If any condition doesn't match the location doesn't match
            foreach (TrackingCondition c in _conditions)
                if (!c.Match(obj))
                    return false;
            //
            // All conditions match
            return true;
        }

        #endregion
    }

    /// <summary>
    /// Contains data that is used to match code locations.
    /// </summary>
    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class UserTrackingLocation
    {
        #region Private Data Members

        private string _keyName = null;

        private Type _argType = null;
        private string _argName = null;
        private bool _trackDerivedArgs = false;

        private Type _activityType = null;
        private string _activityName = null;
        private bool _trackDerivedActivities = false;

        private TrackingConditionCollection _conditions = new TrackingConditionCollection();

        #endregion

        #region Constructors

        public UserTrackingLocation()
        {
        }

        public UserTrackingLocation(Type argumentType)
        {
            _argType = argumentType;
        }

        public UserTrackingLocation(Type argumentType, Type activityType)
        {
            _argType = argumentType;
            _activityType = activityType;
        }

        public UserTrackingLocation(Type argumentType, string activityTypeName)
        {
            _argType = argumentType;
            _activityName = activityTypeName;
        }

        public UserTrackingLocation(string argumentTypeName)
        {
            _argName = argumentTypeName;
        }

        public UserTrackingLocation(string argumentTypeName, string activityTypeName)
        {
            _argName = argumentTypeName;
            _activityName = activityTypeName;
        }

        public UserTrackingLocation(string argumentTypeName, Type activityType)
        {
            _argName = argumentTypeName;

            _activityType = activityType;
        }

        #endregion

        #region Public Properties

        public string KeyName
        {
            get { return _keyName; }
            set { _keyName = value; }
        }

        public Type ArgumentType
        {
            get { return _argType; }
            set { _argType = value; }
        }

        public string ArgumentTypeName
        {
            get { return _argName; }
            set { _argName = value; }
        }

        public bool MatchDerivedArgumentTypes
        {
            get { return _trackDerivedArgs; }
            set { _trackDerivedArgs = value; }
        }

        public Type ActivityType
        {
            get { return _activityType; }
            set { _activityType = value; }
        }

        public string ActivityTypeName
        {
            get { return _activityName; }
            set { _activityName = value; }
        }

        public bool MatchDerivedActivityTypes
        {
            get { return _trackDerivedActivities; }
            set { _trackDerivedActivities = value; }
        }

        public TrackingConditionCollection Conditions
        {
            get { return _conditions; }
        }

        #endregion

        #region Internal Matching Methods

        internal bool Match(Activity activity)
        {
            if (!ActTypeIsMatch(activity))
                return false;
            else
                return ConditionsAreMatch(activity);
        }

        internal bool Match(Activity activity, string keyName, object arg)
        {
            return RuntimeMatch(activity, keyName, arg);
        }

        #endregion

        #region Private Matching Methods

        private bool ActTypeIsMatch(Activity activity)
        {
            if (null != _activityType)
                return TypeMatch.IsMatch(activity, _activityType, _trackDerivedActivities);
            else
                return TypeMatch.IsMatch(activity, _activityName, _trackDerivedActivities);
        }

        private bool RuntimeMatch(Activity activity, string keyName, object obj)
        {
            //
            // Check the excludes - if any exclude matches based on activity only we're not a match
            if (!ActTypeIsMatch(activity))
                return false;
            //
            // Check the name of the key, null means match all
            if (null != _keyName)
            {
                if (0 != String.Compare(_keyName, keyName, StringComparison.Ordinal))
                {
                    return false;
                }
            }

            if (null != _argType)
                return TypeMatch.IsMatch(obj, _argType, _trackDerivedArgs);
            else
                return TypeMatch.IsMatch(obj, _argName, _trackDerivedArgs);
        }

        private bool ConditionsAreMatch(object obj)
        {
            //
            // If any condition doesn't match the location doesn't match
            foreach (TrackingCondition c in _conditions)
                if (!c.Match(obj))
                    return false;
            //
            // All conditions match
            return true;
        }


        #endregion
    }

    internal sealed class TypeMatch
    {
        private TypeMatch() { }

        internal static bool IsMatch(object obj, string name, bool matchDerived)
        {
            Type objType = obj.GetType();
            if (0 == string.Compare(objType.Name, name, StringComparison.Ordinal))
            {
                return true;
            }
            else
            {
                //
                // If we're not checking base types we're done
                if (!matchDerived)
                    return false;
                //
                // Check interfaces (case sensitive)
                // This checks all interfaces (including interfaces on base types )
                if (null != objType.GetInterface(name))
                    return true;
                //
                // Walk down the base types and look for a match
                Type b = objType.BaseType;
                while (b != null)
                {
                    if (0 == string.Compare(b.Name, name, StringComparison.Ordinal))
                        return true;

                    b = b.BaseType;
                }
                return false;
            }
        }

        internal static bool IsMatch(object obj, Type matchType, bool matchDerived)
        {
            Type objType = obj.GetType();
            //
            // First check if the type is a direct match.  
            // Can't just use Type.IsInstanceOfType at this point because that matches bases and interfaces.
            // If not then use IsInstanceOfType to check bases and interfaces if we are matching derived
            if (objType == matchType)
                return true;
            else
            {
                if (matchDerived)
                    return matchType.IsInstanceOfType(obj);
                else
                    return false;
            }
        }
    }
}
