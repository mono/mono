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
    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class TrackingProfile
    {
        #region Private Data Members

        private System.Version _version;

        private ActivityTrackPointCollection _activities = new ActivityTrackPointCollection();
        private UserTrackPointCollection _code = new UserTrackPointCollection();
        private WorkflowTrackPointCollection _instance = new WorkflowTrackPointCollection();

        #endregion

        #region Constructors

        public TrackingProfile()
        {
        }

        #endregion

        #region Public Properties

        public ActivityTrackPointCollection ActivityTrackPoints
        {
            get { return _activities; }
        }

        public UserTrackPointCollection UserTrackPoints
        {
            get { return _code; }
        }

        public WorkflowTrackPointCollection WorkflowTrackPoints
        {
            get { return _instance; }
        }

        public Version Version
        {
            get
            {
                return _version;
            }
            set
            {
                _version = value;
            }
        }

        #endregion

    }
}
