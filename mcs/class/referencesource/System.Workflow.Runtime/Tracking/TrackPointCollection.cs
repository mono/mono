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
    /// Used by TrackingProfile to hold ActivityTrackPoints.
    /// </summary>
    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class ActivityTrackPointCollection : List<ActivityTrackPoint>
    {
        public ActivityTrackPointCollection()
        {
        }

        public ActivityTrackPointCollection(IEnumerable<ActivityTrackPoint> points)
        {
            //
            // Not using the IEnumerable<T> constructor on the base List<T> so that we can check for null.
            // The code behind AddRange doesn't appear to have a significant perf 
            // overhead compared to the IEnumerable<T> constructor if the list is empty
            // (which it will always be at this point).
            if (null == points)
                throw new ArgumentNullException("points");

            AddRange(points);
        }
    }

    /// <summary>
    /// Used by TrackingProfile to hold UserTrackPoints.
    /// </summary>
    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class UserTrackPointCollection : List<UserTrackPoint>
    {
        public UserTrackPointCollection()
        {
        }

        public UserTrackPointCollection(IEnumerable<UserTrackPoint> points)
        {
            //
            // Not using the IEnumerable<T> constructor on the base List<T> so that we can check for null.
            // The code behind AddRange doesn't appear to have a significant perf 
            // overhead compared to the IEnumerable<T> constructor if the list is empty
            // (which it will always be at this point).
            if (null == points)
                throw new ArgumentNullException("points");

            AddRange(points);
        }
    }

    /// <summary>
    /// Used by TrackingProfile to hold ActivityTrackPoints.
    /// </summary>
    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class WorkflowTrackPointCollection : List<WorkflowTrackPoint>
    {
        public WorkflowTrackPointCollection()
        {
        }

        public WorkflowTrackPointCollection(IEnumerable<WorkflowTrackPoint> points)
        {
            //
            // Not using the IEnumerable<T> constructor on the base List<T> so that we can check for null.
            // The code behind AddRange doesn't appear to have a significant perf 
            // overhead compared to the IEnumerable<T> constructor if the list is empty
            // (which it will always be at this point).
            if (null == points)
                throw new ArgumentNullException("points");

            AddRange(points);
        }
    }
}
