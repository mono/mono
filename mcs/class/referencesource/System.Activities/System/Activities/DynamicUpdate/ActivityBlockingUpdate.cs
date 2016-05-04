// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.DynamicUpdate
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.Runtime.Serialization;

    [Serializable]
    [DataContract]
    public class ActivityBlockingUpdate
    {
        [NonSerialized]
        private Activity activity;

        string activityInstanceId;
        string originalActivityId;
        string updatedActivityId;
        string reason;       

        public ActivityBlockingUpdate(Activity activity, string originalActivityId, string reason)
            : this(activity, originalActivityId, reason, null)
        {
        }

        public ActivityBlockingUpdate(Activity activity, string originalActivityId, string reason, string activityInstanceId)
        {
            this.activity = activity;
            this.Reason = reason;
            this.OriginalActivityId = originalActivityId;
            this.ActivityInstanceId = activityInstanceId;
            if (activity != null)
            {
                this.UpdatedActivityId = activity.Id;
            }
        }

        public ActivityBlockingUpdate(string updatedActivityId, string originalActivityId, string reason)
            : this(updatedActivityId, originalActivityId, reason, null)
        {
        }

        public ActivityBlockingUpdate(string updatedActivityId, string originalActivityId, string reason, string activityInstanceId)
        {
            this.UpdatedActivityId = updatedActivityId;
            this.OriginalActivityId = originalActivityId;
            this.ActivityInstanceId = activityInstanceId;
            this.Reason = reason;
        }

        public Activity Activity
        {
            get
            {
                return this.activity;
            }
        }
        
        public string ActivityInstanceId
        {
            get
            {
                return this.activityInstanceId;
            }
            private set
            {
                this.activityInstanceId = value;
            }
        }
        
        public string OriginalActivityId
        {
            get
            {
                return this.originalActivityId;
            }
            private set
            {
                this.originalActivityId = value;
            }
        }
        
        public string UpdatedActivityId
        {
            get
            {
                return this.updatedActivityId;
            }
            private set
            {
                this.updatedActivityId = value;
            }
        }
        
        public string Reason
        {
            get
            {
                return this.reason;
            }
            private set
            {
                this.reason = value;
            }
        }

        [DataMember(EmitDefaultValue = false, Name = "ActivityInstanceId")]
        internal string SerializedActivityInstanceId
        {
            get { return this.ActivityInstanceId; }
            set { this.ActivityInstanceId = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "OriginalActivityId")]
        internal string SerializedOriginalActivityId
        {
            get { return this.OriginalActivityId; }
            set { this.OriginalActivityId = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "UpdatedActivityId")]
        internal string SerializedUpdatedActivityId
        {
            get { return this.UpdatedActivityId; }
            set { this.UpdatedActivityId = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "Reason")]
        internal string SerializedReason
        {
            get { return this.Reason; }
            set { this.Reason = value; }
        }

        internal static void AddBlockingActivity(ref Collection<ActivityBlockingUpdate> blockingActivities, Activity activity, string originalActivityId, string reason, string activityInstanceId)
        {
            if (blockingActivities == null)
            {
                blockingActivities = new Collection<ActivityBlockingUpdate>();
            }

            ActivityBlockingUpdate blockingActivity = new ActivityBlockingUpdate(activity, originalActivityId, reason, activityInstanceId);
            blockingActivities.Add(blockingActivity);
        }

        internal static void AddBlockingActivity(ref Collection<ActivityBlockingUpdate> blockingActivities, string updatedActivityId, string originalActivityId, string reason, string activityInstanceId)
        {
            if (blockingActivities == null)
            {
                blockingActivities = new Collection<ActivityBlockingUpdate>();
            }

            ActivityBlockingUpdate blockingActivity = new ActivityBlockingUpdate(updatedActivityId, originalActivityId, reason, activityInstanceId);
            blockingActivities.Add(blockingActivity);
        }
    }
}
