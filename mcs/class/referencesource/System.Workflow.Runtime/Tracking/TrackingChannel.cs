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
    /// Base class from which all tracking channels must derive in order to receive tracking data.
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public abstract class TrackingChannel
    {
        protected internal abstract void Send(TrackingRecord record);
        protected internal abstract void InstanceCompletedOrTerminated();
    }
}
