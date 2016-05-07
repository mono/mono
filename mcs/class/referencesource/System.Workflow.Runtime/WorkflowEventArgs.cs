// ****************************************************************************
// Copyright (C) Microsoft Corporation.  All rights reserved.
//

using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using System.Workflow.Runtime;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime.Hosting;

namespace System.Workflow.Runtime
{

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class WorkflowEventArgs : EventArgs
    {
        private WorkflowInstance _instance;

        internal WorkflowEventArgs(WorkflowInstance instance)
        {
            _instance = instance;
        }

        public WorkflowInstance WorkflowInstance
        {
            get
            {
                return _instance;
            }
        }
    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class WorkflowCompletedEventArgs : WorkflowEventArgs
    {
        private Dictionary<String, Object> _outputParameters;
        private Activity _originalWorkflowDefinition;
        private Activity _workflowDefinition;

        internal WorkflowCompletedEventArgs(WorkflowInstance instance, Activity workflowDefinition)
            : base(instance)
        {
            this._outputParameters = new Dictionary<String, Object>();
            this._originalWorkflowDefinition = workflowDefinition;
            this._workflowDefinition = null;
        }

        public Dictionary<String, Object> OutputParameters
        {
            get
            {
                return this._outputParameters;
            }
        }

        public Activity WorkflowDefinition
        {
            get
            {
                if (this._workflowDefinition == null)
                {
                    using (new WorkflowDefinitionLock(this._originalWorkflowDefinition))
                    {
                        if (this._workflowDefinition == null)
                        {
                            // Clone the original definition after locking the
                            // definition's [....] object which was passed in
                            // the constructor.  This is so that the host cannot
                            // corrupt the shared definition
                            Activity tempDefinition = this._originalWorkflowDefinition.Clone();
                            Thread.MemoryBarrier();
                            this._workflowDefinition = tempDefinition;
                        }
                    }
                }

                return this._workflowDefinition;
            }
        }
    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class WorkflowSuspendedEventArgs : WorkflowEventArgs
    {
        private String _error;

        internal WorkflowSuspendedEventArgs(WorkflowInstance instance, String error)
            : base(instance)
        {
            this._error = error;
        }

        public String Error
        {
            get
            {
                return this._error;
            }
        }
    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class WorkflowTerminatedEventArgs : WorkflowEventArgs
    {
        private Exception exception;

        internal WorkflowTerminatedEventArgs(WorkflowInstance instance, String error)
            : base(instance)
        {
            this.exception = new WorkflowTerminatedException(error);
        }
        internal WorkflowTerminatedEventArgs(WorkflowInstance instance, Exception e)
            : base(instance)
        {
            this.exception = e;
        }

        public Exception Exception
        {
            get
            {
                return this.exception;
            }
        }
    }

    internal sealed class WorkflowDefinitionEventArgs : EventArgs
    {
        private Type _workflowType;
        private byte[] _xomlHashCode;

        internal WorkflowDefinitionEventArgs(Type scheduleType)
        {
            _workflowType = scheduleType;
        }

        internal WorkflowDefinitionEventArgs(byte[] scheduleDefHash)
        {
            _xomlHashCode = scheduleDefHash;
        }

        public Type WorkflowType
        {
            get
            {
                return _workflowType;
            }
        }

        public byte[] WorkflowDefinitionHashCode
        {
            get
            {
                return _xomlHashCode;
            }
        }
    }
}
