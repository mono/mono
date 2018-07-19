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
    /// Base class for tracking services.
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public abstract class TrackingService : WorkflowRuntimeService
    {
        protected internal abstract TrackingChannel GetTrackingChannel(TrackingParameters parameters);

        protected internal abstract bool TryGetProfile(Type workflowType, out TrackingProfile profile);

        protected internal abstract TrackingProfile GetProfile(Type workflowType, Version profileVersionId);

        protected internal abstract TrackingProfile GetProfile(Guid workflowInstanceId);

        protected internal abstract bool TryReloadProfile(Type workflowType, Guid workflowInstanceId, out TrackingProfile profile);
    }
}
