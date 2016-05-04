#region Imports

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Reflection;
using System.Threading;
using System.Globalization;
using System.IO;
using System.Workflow.Runtime.Hosting;
using System.Workflow.Runtime.Configuration;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime.Tracking;
using System.Workflow.ComponentModel.Compiler;
using System.Xml;
using System.Workflow.Runtime.DebugEngine;
using System.Workflow.ComponentModel.Serialization;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;

#endregion

namespace System.Workflow.Runtime
{
    #region Class ServicesExceptionNotHandledEventArgs

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class ServicesExceptionNotHandledEventArgs : EventArgs
    {
        private Exception exception;
        private Guid instanceId;

        internal ServicesExceptionNotHandledEventArgs(Exception exception, Guid instanceId)
        {
            this.exception = exception;
            this.instanceId = instanceId;
        }

        public Exception Exception
        {
            get { return exception; }
        }

        public Guid WorkflowInstanceId
        {
            get { return instanceId; }
        }
    }

    #endregion
}
