//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities.Tracking
{
    using System;
    using System.Runtime;
    using System.Activities.Tracking;

    abstract class TrackingProfileManager
    {
        protected TrackingProfileManager()
        {
        }

        public virtual IAsyncResult BeginLoad(
            string profileName,
            string activityDefinitionId,
            TimeSpan timeout,
            AsyncCallback callback,
            object state)
        {
            TrackingProfile profile = Load(profileName, activityDefinitionId, timeout);
            return new CompletedAsyncResult<TrackingProfile>(profile, callback, state);
        }

        public abstract TrackingProfile Load(
            string profileName,
            string activityDefinitionId,
            TimeSpan timeout);

        public virtual TrackingProfile EndLoad(IAsyncResult result)
        {
            return CompletedAsyncResult<TrackingProfile>.End(result);
        }
    }
}
