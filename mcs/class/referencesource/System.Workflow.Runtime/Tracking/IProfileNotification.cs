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
    /// Interface for tracking services that provide notifications when a new version of a profile is availabe.  
    /// The tracking runtime subscribes to the UpdateProfile and RemoveProfile events and updates its cache as events are raised.  
    /// This decreases the number of requests for profiles that are made to a tracking service.  The GetProfiles methods 
    /// are still in use but they are not called as frequently.
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public interface IProfileNotification
    {
        /// <summary>
        /// Use this event to inform the tracking runtime that a new profile version is available for a given workflow type.
        /// </summary>
        event EventHandler<ProfileUpdatedEventArgs> ProfileUpdated;
        /// <summary>
        /// Use this event to inform the tracking runtime that new instances of the specified workflow type should not have a profile.
        /// </summary>
        event EventHandler<ProfileRemovedEventArgs> ProfileRemoved;
    }
}
