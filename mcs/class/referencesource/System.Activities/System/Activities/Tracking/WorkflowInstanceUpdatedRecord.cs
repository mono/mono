//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Tracking
{
    using System;
    using System.Activities.DynamicUpdate;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Runtime;
    using System.Globalization;

    [Fx.Tag.XamlVisible(false)]
    [DataContract]
    public sealed class WorkflowInstanceUpdatedRecord : WorkflowInstanceRecord
    {
        WorkflowIdentity originalDefinitionIdentity;
        IList<ActivityBlockingUpdate> blockingActivities;

        public WorkflowInstanceUpdatedRecord(Guid instanceId, string activityDefinitionId, WorkflowIdentity originalDefinitionIdentity, WorkflowIdentity updatedDefinitionIdentity)
            : base(instanceId, activityDefinitionId, WorkflowInstanceStates.Updated, updatedDefinitionIdentity)
        {
            this.OriginalDefinitionIdentity = originalDefinitionIdentity;
        }

        public WorkflowInstanceUpdatedRecord(Guid instanceId, string activityDefinitionId, WorkflowIdentity  originalDefinitionIdentity, WorkflowIdentity updatedDefinitionIdentity, IList<ActivityBlockingUpdate> blockingActivities)
            : base(instanceId, activityDefinitionId, WorkflowInstanceStates.UpdateFailed, updatedDefinitionIdentity)
        {
            this.OriginalDefinitionIdentity = originalDefinitionIdentity;
            this.BlockingActivities = new List<ActivityBlockingUpdate>(blockingActivities).AsReadOnly();
        }

        public WorkflowInstanceUpdatedRecord(Guid instanceId, long recordNumber, string activityDefinitionId, WorkflowIdentity originalDefinitionIdentity, WorkflowIdentity updatedDefinitionIdentity)
            : base(instanceId, recordNumber, activityDefinitionId, WorkflowInstanceStates.Updated, updatedDefinitionIdentity)
        {
            this.OriginalDefinitionIdentity = originalDefinitionIdentity;
        }

        public WorkflowInstanceUpdatedRecord(Guid instanceId, long recordNumber, string activityDefinitionId, WorkflowIdentity originalDefinitionIdentity, WorkflowIdentity updatedDefinitionIdentity, IList<ActivityBlockingUpdate> blockingActivities)
            : base(instanceId, recordNumber, activityDefinitionId, WorkflowInstanceStates.UpdateFailed, updatedDefinitionIdentity)
        {
            this.OriginalDefinitionIdentity = originalDefinitionIdentity;
            this.BlockingActivities = new List<ActivityBlockingUpdate>(blockingActivities).AsReadOnly();
        }

        WorkflowInstanceUpdatedRecord(WorkflowInstanceUpdatedRecord record)
            : base(record)
        {
            this.OriginalDefinitionIdentity = record.OriginalDefinitionIdentity;
            this.BlockingActivities = record.BlockingActivities;
        }
        
        public WorkflowIdentity OriginalDefinitionIdentity
        {
            get
            {
                return this.originalDefinitionIdentity;
            }
            private set
            {
                this.originalDefinitionIdentity = value;
            }
        }

        public bool IsSuccessful
        {
            get
            {
                return this.BlockingActivities == null;
            }
        }
        
        public IList<ActivityBlockingUpdate> BlockingActivities
        {
            get
            {
                return this.blockingActivities;
            }
            private set
            {
                this.blockingActivities = value;
            }
        }

        [DataMember(Name = "OriginalDefinitionIdentity")]
        internal WorkflowIdentity SerializedOriginalDefinitionIdentity
        {
            get { return this.OriginalDefinitionIdentity; }
            set { this.OriginalDefinitionIdentity = value; }
        }

        [DataMember(Name = "BlockingActivities")]
        internal IList<ActivityBlockingUpdate> SerializedBlockingActivities
        {
            get { return this.BlockingActivities; }
            set { this.BlockingActivities = value; }
        }

        protected internal override TrackingRecord Clone()
        {
            return new WorkflowInstanceUpdatedRecord(this);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture,
                "WorkflowInstanceUpdatedRecord {{ InstanceId = {0}, RecordNumber = {1}, EventTime = {2}, ActivityDefinitionId = {3}, State = {4}, OriginalDefinitionIdentity = {5}, UpdatedDefinitionIdentity = {6}, IsSuccessful = {7} }} ",
                this.InstanceId,
                this.RecordNumber,
                this.EventTime,
                this.ActivityDefinitionId,
                this.State,
                this.OriginalDefinitionIdentity,
                this.WorkflowDefinitionIdentity,
                this.IsSuccessful);
        }        
    }
}
