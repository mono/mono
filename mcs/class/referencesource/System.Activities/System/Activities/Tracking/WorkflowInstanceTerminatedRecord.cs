//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Tracking
{
    using System;
    using System.Runtime.Serialization;
    using System.Runtime;
    using System.Diagnostics;
    using System.Globalization;

    [Fx.Tag.XamlVisible(false)]
    [DataContract]
    public sealed class WorkflowInstanceTerminatedRecord : WorkflowInstanceRecord
    {
        string reason;

        public WorkflowInstanceTerminatedRecord(Guid instanceId, string activityDefinitionId, string reason)
            : base(instanceId, activityDefinitionId, WorkflowInstanceStates.Terminated)
        {
            if (string.IsNullOrEmpty(reason))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("reason");
            }
            this.Reason = reason;
            this.Level = TraceLevel.Error;
        }

        public WorkflowInstanceTerminatedRecord(Guid instanceId, long recordNumber, string activityDefinitionId, string reason)
            : base(instanceId, recordNumber, activityDefinitionId, WorkflowInstanceStates.Terminated)
        {
            if (string.IsNullOrEmpty(reason))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("reason");
            }

            this.Reason = reason;
            this.Level = TraceLevel.Error;
        }

        public WorkflowInstanceTerminatedRecord(Guid instanceId, string activityDefinitionId, string reason, WorkflowIdentity workflowDefinitionIdentity)
            : this(instanceId, activityDefinitionId, reason)
        {
            this.WorkflowDefinitionIdentity = workflowDefinitionIdentity;
        }

        public WorkflowInstanceTerminatedRecord(Guid instanceId, long recordNumber, string activityDefinitionId, string reason, WorkflowIdentity workflowDefinitionIdentity)
            : this(instanceId, recordNumber, activityDefinitionId, reason)
        {
            this.WorkflowDefinitionIdentity = workflowDefinitionIdentity;
        }

        WorkflowInstanceTerminatedRecord(WorkflowInstanceTerminatedRecord record)
            : base(record)
        {
            this.Reason = record.Reason;
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

        [DataMember(Name = "Reason")]
        internal string SerializedReason
        {
            get { return this.Reason; }
            set { this.Reason = value; }
        }

        protected internal override TrackingRecord Clone()
        {
            return new WorkflowInstanceTerminatedRecord(this);
        }

        public override string ToString()
        {
            // For backward compatibility, the ToString() does not return 
            // WorkflowIdentity, if it is null.
            if (this.WorkflowDefinitionIdentity == null)
            {
                return string.Format(CultureInfo.CurrentCulture,
                    "WorkflowInstanceTerminatedRecord {{ InstanceId = {0}, RecordNumber = {1}, EventTime = {2}, ActivityDefinitionId = {3}, Reason = {4} }} ",
                    this.InstanceId,
                    this.RecordNumber,
                    this.EventTime,
                    this.ActivityDefinitionId,
                    this.Reason);
            }
            else
            {
                return string.Format(CultureInfo.CurrentCulture,
                    "WorkflowInstanceTerminatedRecord {{ InstanceId = {0}, RecordNumber = {1}, EventTime = {2}, ActivityDefinitionId = {3}, Reason = {4}, WorkflowDefinitionIdentity = {5} }} ",
                    this.InstanceId,
                    this.RecordNumber,
                    this.EventTime,
                    this.ActivityDefinitionId,
                    this.Reason,
                    this.WorkflowDefinitionIdentity);
            }
        }
    }
}
