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
    /// Manages the runtime's cache of tracking profiles.
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public static class TrackingProfileCache
    {
        public static void Clear()
        {
            TrackingProfileManager.ClearCache();
        }
    }
}
