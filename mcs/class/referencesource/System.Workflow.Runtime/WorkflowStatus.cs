#region Imports

using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading; 
using System.Transactions;
using System.Reflection;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime.Hosting;
using System.Diagnostics;

#endregion

namespace System.Workflow.Runtime
{

    #region Enum WorkflowStatus

    // don't change the indices for the values since the persistence provider
    // depends on the indices.
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public enum WorkflowStatus
    {
        Running = 0,
        Completed = 1,
        Suspended = 2,
        Terminated = 3,
        Created = 4
    }

    #endregion
}
