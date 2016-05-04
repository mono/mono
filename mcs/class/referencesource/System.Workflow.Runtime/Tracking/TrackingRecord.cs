using System;
using System.Collections;
using System.Collections.Generic;
using System.Workflow.ComponentModel;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Workflow.Runtime.Hosting;

namespace System.Workflow.Runtime.Tracking
{
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public abstract class TrackingRecord
    {
        protected TrackingRecord()
        {
        }

        public abstract DateTime EventDateTime
        {
            get;
            set;
        }

        public abstract int EventOrder
        {
            get;
            set;
        }

        public abstract EventArgs EventArgs
        {
            get;
            set;
        }

        public abstract TrackingAnnotationCollection Annotations
        {
            get;
        }
    }

    /// <summary>
    /// Contains data for a specific extraction point.
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class ActivityTrackingRecord : TrackingRecord
    {
        #region Data Members

        private string _qualifiedID = null;
        private Type _activityType = null;
        private ActivityExecutionStatus _status;
        private List<TrackingDataItem> _body = new List<TrackingDataItem>();
        private Guid _contextGuid = Guid.Empty, _parentContextGuid = Guid.Empty;

        private DateTime _eventDateTime = DateTime.MinValue;
        private int _eventOrder = -1;
        private EventArgs _args = null;
        private TrackingAnnotationCollection _annotations = new TrackingAnnotationCollection();

        #endregion

        #region Constructors

        public ActivityTrackingRecord()
        {
        }

        public ActivityTrackingRecord(Type activityType, string qualifiedName, Guid contextGuid, Guid parentContextGuid, ActivityExecutionStatus executionStatus, DateTime eventDateTime, int eventOrder, EventArgs eventArgs)
        {
            _activityType = activityType;
            _qualifiedID = qualifiedName;
            _status = executionStatus;
            _eventDateTime = eventDateTime;
            _contextGuid = contextGuid;
            _parentContextGuid = parentContextGuid;
            _eventOrder = eventOrder;
            _args = eventArgs;
        }

        #endregion

        #region Public Properties

        public string QualifiedName
        {
            get { return _qualifiedID; }
            set { _qualifiedID = value; }
        }

        public Guid ContextGuid
        {
            get { return _contextGuid; }
            set { _contextGuid = value; }
        }

        public Guid ParentContextGuid
        {
            get { return _parentContextGuid; }
            set { _parentContextGuid = value; }
        }

        public Type ActivityType
        {
            get { return _activityType; }
            set { _activityType = value; }
        }

        public ActivityExecutionStatus ExecutionStatus
        {
            get { return _status; }
            set { _status = value; }
        }

        public IList<TrackingDataItem> Body
        {
            get { return _body; }
        }

        #endregion

        #region TrackingRecord

        public override DateTime EventDateTime
        {
            get { return _eventDateTime; }
            set { _eventDateTime = value; }
        }
        /// <summary>
        /// Contains a value indicating the relative order of this event within the context of a workflow instance.  
        /// Value will be unique within a workflow instance but is not guaranteed to be sequential.
        /// </summary>
        public override int EventOrder
        {
            get { return _eventOrder; }
            set { _eventOrder = value; }
        }

        public override EventArgs EventArgs
        {
            get { return _args; }
            set { _args = value; }
        }

        public override TrackingAnnotationCollection Annotations
        {
            get { return _annotations; }
        }

        #endregion
    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class UserTrackingRecord : TrackingRecord
    {
        #region Data Members

        private string _qualifiedID = null;
        private Type _activityType = null;
        private List<TrackingDataItem> _body = new List<TrackingDataItem>();
        private Guid _contextGuid = Guid.Empty, _parentContextGuid = Guid.Empty;

        private DateTime _eventDateTime = DateTime.MinValue;
        private int _eventOrder = -1;
        private object _userData = null;
        private TrackingAnnotationCollection _annotations = new TrackingAnnotationCollection();
        private EventArgs _args = null;
        private string _key = null;

        #endregion

        #region Constructors

        public UserTrackingRecord()
        {
        }

        public UserTrackingRecord(Type activityType, string qualifiedName, Guid contextGuid, Guid parentContextGuid, DateTime eventDateTime, int eventOrder, string userDataKey, object userData)
        {
            _activityType = activityType;
            _qualifiedID = qualifiedName;
            _eventDateTime = eventDateTime;
            _contextGuid = contextGuid;
            _parentContextGuid = parentContextGuid;
            _eventOrder = eventOrder;
            _userData = userData;
            _key = userDataKey;
        }

        #endregion

        #region Public Properties

        public string QualifiedName
        {
            get { return _qualifiedID; }
            set { _qualifiedID = value; }
        }

        public Guid ContextGuid
        {
            get { return _contextGuid; }
            set { _contextGuid = value; }
        }

        public Guid ParentContextGuid
        {
            get { return _parentContextGuid; }
            set { _parentContextGuid = value; }
        }

        public Type ActivityType
        {
            get { return _activityType; }
            set { _activityType = value; }
        }

        public IList<TrackingDataItem> Body
        {
            get { return _body; }
        }

        public string UserDataKey
        {
            get { return _key; }
            set { _key = value; }
        }

        public object UserData
        {
            get { return _userData; }
            set { _userData = value; }
        }

        #endregion

        #region TrackingRecord

        public override DateTime EventDateTime
        {
            get { return _eventDateTime; }
            set { _eventDateTime = value; }
        }
        /// <summary>
        /// Contains a value indicating the relative order of this event within the context of a workflow instance.  
        /// Value will be unique within a workflow instance but is not guaranteed to be sequential.
        /// </summary>
        public override int EventOrder
        {
            get { return _eventOrder; }
            set { _eventOrder = value; }
        }

        public override TrackingAnnotationCollection Annotations
        {
            get { return _annotations; }
        }

        public override EventArgs EventArgs
        {
            get { return _args; }
            set { _args = value; }
        }

        #endregion
    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class WorkflowTrackingRecord : TrackingRecord
    {
        #region Private Data Members

        private TrackingWorkflowEvent _event;
        private DateTime _eventDateTime = DateTime.MinValue;
        private int _eventOrder = -1;
        private EventArgs _args = null;
        private TrackingAnnotationCollection _annotations = new TrackingAnnotationCollection();

        #endregion

        #region Constructors

        public WorkflowTrackingRecord()
        {
        }

        public WorkflowTrackingRecord(TrackingWorkflowEvent trackingWorkflowEvent, DateTime eventDateTime, int eventOrder, EventArgs eventArgs)
        {
            _event = trackingWorkflowEvent;
            _eventDateTime = eventDateTime;
            _eventOrder = eventOrder;
            _args = eventArgs;
        }

        #endregion

        #region TrackingRecord

        public TrackingWorkflowEvent TrackingWorkflowEvent
        {
            get { return _event; }
            set { _event = value; }
        }

        public override DateTime EventDateTime
        {
            get { return _eventDateTime; }
            set { _eventDateTime = value; }
        }
        /// <summary>
        /// Contains a value indicating the relative order of this event within the context of a workflow instance.  
        /// Value will be unique within a workflow instance but is not guaranteed to be sequential.
        /// </summary>
        public override int EventOrder
        {
            get { return _eventOrder; }
            set { _eventOrder = value; }
        }

        public override EventArgs EventArgs
        {
            get { return _args; }
            set { _args = value; }
        }

        public override TrackingAnnotationCollection Annotations
        {
            get { return _annotations; }
        }

        #endregion
    }
}
