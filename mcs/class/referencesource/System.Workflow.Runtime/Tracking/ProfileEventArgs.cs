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
    /// EventArgs for IProfileNotification.ProfileUpdated event.
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class ProfileUpdatedEventArgs : EventArgs
    {
        private TrackingProfile _profile = null;
        private Type _workflowType = null;

        public ProfileUpdatedEventArgs() { }

        public ProfileUpdatedEventArgs(Type workflowType, TrackingProfile profile)
        {
            _workflowType = workflowType;
            _profile = profile;
        }

        public TrackingProfile TrackingProfile
        {
            get { return _profile; }
            set { _profile = value; }
        }

        public Type WorkflowType
        {
            get { return _workflowType; }
            set { _workflowType = value; }
        }
    }
    /// <summary>
    /// EventArgs for IProfileNotification.ProfileRemoved event.
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class ProfileRemovedEventArgs : EventArgs
    {
        private Type _workflowType = null;

        public ProfileRemovedEventArgs() { }

        public ProfileRemovedEventArgs(Type workflowType)
        {
            _workflowType = workflowType;
        }

        public Type WorkflowType
        {
            get { return _workflowType; }
            set { _workflowType = value; }
        }
    }
}
