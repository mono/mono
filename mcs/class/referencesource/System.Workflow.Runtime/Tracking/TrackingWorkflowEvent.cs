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
    /// Events for workflow instances.
    /// </summary>
    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public enum TrackingWorkflowEvent
    {
        Created = 0,
        Completed = 1,
        Idle = 2,
        Suspended = 3,
        Resumed = 4,
        Persisted = 5,
        Unloaded = 6,
        Loaded = 7,
        Exception = 8,
        Terminated = 9,
        Aborted = 10,
        Changed = 11,
        Started = 12
    }
}
