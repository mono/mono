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
    /// Abstract base for classes that extract data
    /// </summary>
    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public abstract class TrackingExtract
    {
        public abstract TrackingAnnotationCollection Annotations { get; }
        public abstract string Member { get; set; }
        internal abstract void GetData(Activity activity, IServiceProvider provider, IList<TrackingDataItem> items);
    }

    /// <summary>
    /// Used to extract data members from a workflow's code separation partial class.
    /// </summary>
    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class WorkflowDataTrackingExtract : TrackingExtract
    {
        #region Private DataMembers

        private string _name = null;
        private TrackingAnnotationCollection _annotations = new TrackingAnnotationCollection();

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public WorkflowDataTrackingExtract()
        {
        }
        /// <summary>
        /// Construct with a Member list.
        /// </summary>
        /// <param name="member">List of "." delineated property names</param>
        public WorkflowDataTrackingExtract(string member)
        {
            _name = member;
        }

        #endregion

        #region TrackingExtract

        public override string Member
        {
            get { return _name; }
            set { _name = value; }
        }

        public override TrackingAnnotationCollection Annotations
        {
            get { return _annotations; }
        }

        internal override void GetData(Activity activity, IServiceProvider provider, IList<TrackingDataItem> items)
        {
            Activity root = ContextActivityUtils.RootContextActivity(activity);

            if ((null == _name) || (0 == _name.Trim().Length))
            {
                //
                // If we don't have a name we get everything
                PropertyHelper.GetAllMembers(root, items, _annotations);
            }
            else
            {
                TrackingDataItem item = null;
                PropertyHelper.GetProperty(_name, root, _annotations, out item);
                if (null != item)
                    items.Add(item);
            }
        }

        #endregion
    }
    /// <summary>
    /// Used to extract data members from an activity in a workflow instance.
    /// </summary>
    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class ActivityDataTrackingExtract : TrackingExtract
    {
        #region Private DataMembers

        private string _name = null;
        private TrackingAnnotationCollection _annotations = new TrackingAnnotationCollection();

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ActivityDataTrackingExtract()
        {
        }
        /// <summary>
        /// Construct with a Member list.
        /// </summary>
        /// <param name="member">List of "." delineated property names</param>
        public ActivityDataTrackingExtract(string member)
        {
            _name = member;
        }

        #endregion

        #region TrackingExtract

        public override string Member
        {
            get { return _name; }
            set { _name = value; }
        }

        public override TrackingAnnotationCollection Annotations
        {
            get { return _annotations; }
        }

        internal override void GetData(Activity activity, IServiceProvider provider, IList<TrackingDataItem> items)
        {

            if ((null == _name) || (0 == _name.Trim().Length))
            {
                //
                // If we don't have a name we get everything
                PropertyHelper.GetAllMembers(activity, items, _annotations);
            }
            else
            {
                TrackingDataItem item = null;
                PropertyHelper.GetProperty(_name, activity, _annotations, out item);
                if (null != item)
                    items.Add(item);
            }
        }
        #endregion
    }
}
