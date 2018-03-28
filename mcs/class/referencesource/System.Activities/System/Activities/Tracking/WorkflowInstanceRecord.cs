//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Tracking
{
    using System;
    using System.Runtime.Serialization;
    using System.Runtime;
    using System.Globalization;

    [Fx.Tag.XamlVisible(false)]
    [DataContract]
    public class WorkflowInstanceRecord : TrackingRecord
    {
        WorkflowIdentity workflowDefinitionIdentity;
        string state;
        string activityDefinitionId;

        public WorkflowInstanceRecord(Guid instanceId, string activityDefinitionId, string state)
            : base(instanceId)
        {
            if (string.IsNullOrEmpty(activityDefinitionId))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("activityDefinitionId");
            }
            if (string.IsNullOrEmpty(state))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("state");
            }
            this.ActivityDefinitionId = activityDefinitionId;
            this.State = state;
        }

        public WorkflowInstanceRecord(Guid instanceId, long recordNumber, string activityDefinitionId, string state)
            : base(instanceId, recordNumber)
        {
            if (string.IsNullOrEmpty(activityDefinitionId))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("activityDefinitionId");
            }
            if (string.IsNullOrEmpty(state))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("state");
            }
            this.ActivityDefinitionId = activityDefinitionId;
            this.State = state;
        }

        public WorkflowInstanceRecord(Guid instanceId, string activityDefinitionId, string state, WorkflowIdentity workflowDefinitionIdentity)
            : this(instanceId, activityDefinitionId, state)
        {
            this.WorkflowDefinitionIdentity = workflowDefinitionIdentity;
        }

        public WorkflowInstanceRecord(Guid instanceId, long recordNumber, string activityDefinitionId, string state, WorkflowIdentity workflowDefinitionIdentity)
            : this(instanceId, recordNumber, activityDefinitionId, state)
        {
            this.WorkflowDefinitionIdentity = workflowDefinitionIdentity;
        }

        protected WorkflowInstanceRecord(WorkflowInstanceRecord record)
            : base(record)
        {
            this.ActivityDefinitionId = record.ActivityDefinitionId;
            this.State = record.State;
            this.WorkflowDefinitionIdentity = record.WorkflowDefinitionIdentity;
        }
        
        public WorkflowIdentity WorkflowDefinitionIdentity
        {
            get
            {
                return this.workflowDefinitionIdentity;
            }
            protected set
            {
                this.workflowDefinitionIdentity = value;
            }
        }
        
        public string State
        {
            get
            {
                return this.state;
            }
            private set
            {
                this.state = value;
            }
        }
        
        public string ActivityDefinitionId
        {
            get
            {
                return this.activityDefinitionId;
            }
            private set
            {
                this.activityDefinitionId = value;
            }
        }

        [DataMember(Name = "WorkflowDefinitionIdentity")]
        internal WorkflowIdentity SerializedWorkflowDefinitionIdentity
        {
            get { return this.WorkflowDefinitionIdentity; }
            set { this.WorkflowDefinitionIdentity = value; }
        }

        [DataMember(Name = "State")]
        internal string SerializedState
        {
            get { return this.State; }
            set { this.State = value; }
        }

        [DataMember(Name = "ActivityDefinitionId")]
        internal string SerializedActivityDefinitionId
        {
            get { return this.ActivityDefinitionId; }
            set { this.ActivityDefinitionId = value; }
        }

        protected internal override TrackingRecord Clone()
        {
            return new WorkflowInstanceRecord(this);
        }

        public override string ToString()
        {
            // For backward compatibility, the ToString() does not return 
            // WorkflowIdentity, if it is null.
            if (this.WorkflowDefinitionIdentity == null)
            {
                return string.Format(CultureInfo.CurrentCulture,
                    "WorkflowInstanceRecord {{ {0}, ActivityDefinitionId = {1}, State = {2} }}",
                    base.ToString(),
                    this.ActivityDefinitionId,
                    this.State);
            }
            else
            {
                return string.Format(CultureInfo.CurrentCulture,
                    "WorkflowInstanceRecord {{ {0}, ActivityDefinitionId = {1}, State = {2}, WorkflowDefinitionIdentity = {3} }}",
                    base.ToString(),
                    this.ActivityDefinitionId,
                    this.State,
                    this.WorkflowDefinitionIdentity);
            }
        }

    }
}
