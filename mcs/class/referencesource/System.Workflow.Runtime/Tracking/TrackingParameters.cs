using System;
using System.Collections;
using System.Collections.Generic;
using System.Workflow.ComponentModel;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Workflow.Runtime.Hosting;

namespace System.Workflow.Runtime.Tracking
{
    /// <summary>
    /// Contains data useful when construction tracking channels
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class TrackingParameters
    {
        private Guid _instanceId = Guid.Empty;
        private Guid _callerInstanceId = Guid.Empty;
        private Type _workflowType = null;
        private IList<string> _activityCallPath = null;
        private Guid _contextGuid = Guid.Empty, _callerContextGuid = Guid.Empty, _callerParentContextGuid = Guid.Empty;
        private Activity _rootActivity = null;

        private TrackingParameters()
        {
        }

        public TrackingParameters(Guid instanceId, Type workflowType, Activity rootActivity, IList<string> callPath, Guid callerInstanceId, Guid contextGuid, Guid callerContextGuid, Guid callerParentContextGuid)
        {
            _instanceId = instanceId;
            _workflowType = workflowType;
            _activityCallPath = callPath;
            _callerInstanceId = callerInstanceId;
            _contextGuid = contextGuid;
            _callerContextGuid = callerContextGuid;
            _callerParentContextGuid = callerParentContextGuid;
            _rootActivity = rootActivity;
        }

        public Guid InstanceId
        {
            get { return _instanceId; }
        }

        public Type WorkflowType
        {
            get { return _workflowType; }
        }

        public Activity RootActivity
        {
            get { return _rootActivity; }
        }

        public IList<string> CallPath
        {
            get { return _activityCallPath; }
        }

        public Guid CallerInstanceId
        {
            get { return _callerInstanceId; }
        }

        public Guid ContextGuid
        {
            get { return _contextGuid; }
        }

        public Guid CallerContextGuid
        {
            get { return _callerContextGuid; }
        }

        public Guid CallerParentContextGuid
        {
            get { return _callerParentContextGuid; }
        }
    }
}

