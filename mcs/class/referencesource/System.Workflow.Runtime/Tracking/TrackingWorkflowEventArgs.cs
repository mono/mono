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
    public class TrackingWorkflowChangedEventArgs : EventArgs
    {
        private Activity _def = null;
        private IList<WorkflowChangeAction> _changes = null;

        internal TrackingWorkflowChangedEventArgs(IList<WorkflowChangeAction> changes, Activity definition)
        {
            _def = definition;
            _changes = changes;
        }

        public IList<WorkflowChangeAction> Changes
        {
            get { return _changes; }
        }

        public Activity Definition
        {
            get { return _def; }
        }
    }


    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class TrackingWorkflowTerminatedEventArgs : EventArgs
    {
        private Exception _e = null;

        internal TrackingWorkflowTerminatedEventArgs(Exception exception)
        {
            _e = exception;
        }

        internal TrackingWorkflowTerminatedEventArgs(string error)
        {
            _e = new WorkflowTerminatedException(error);
        }

        public Exception Exception
        {
            get { return _e; }
        }
    }

    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class TrackingWorkflowSuspendedEventArgs : EventArgs
    {
        private string _error = null;

        internal TrackingWorkflowSuspendedEventArgs(string error)
        {
            _error = error;
        }

        public string Error
        {
            get { return _error; }
        }
    }


    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class TrackingWorkflowExceptionEventArgs : EventArgs
    {
        private Exception _e = null;
        private string _currentPath = null;
        private string _originalPath = null;
        private Guid _context, _parentContext;

        internal TrackingWorkflowExceptionEventArgs(Exception exception, string currentPath, string originalPath, Guid contextGuid, Guid parentContextGuid)
        {
            _e = exception;
            _currentPath = currentPath;
            _originalPath = originalPath;
            _context = contextGuid;
            _parentContext = parentContextGuid;
        }

        public Exception Exception
        {
            get { return _e; }
        }

        public string CurrentActivityPath
        {
            get { return _currentPath; }
        }

        public string OriginalActivityPath
        {
            get { return _originalPath; }
        }

        public Guid ContextGuid
        {
            get { return _context; }
        }

        public Guid ParentContextGuid
        {
            get { return _parentContext; }
        }
    }
}
