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
    public sealed class WorkflowInstanceUnhandledExceptionRecord : WorkflowInstanceRecord
    {
        Exception unhandledException;
        ActivityInfo faultSource;

        public WorkflowInstanceUnhandledExceptionRecord(Guid instanceId, string activityDefinitionId, ActivityInfo faultSource, Exception exception)
            : this(instanceId, 0, activityDefinitionId, faultSource, exception)
        {
        }

        public WorkflowInstanceUnhandledExceptionRecord(Guid instanceId, long recordNumber, string activityDefinitionId, ActivityInfo faultSource, Exception exception)
            : base(instanceId, recordNumber, activityDefinitionId, WorkflowInstanceStates.UnhandledException)
        {
            if (string.IsNullOrEmpty(activityDefinitionId))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("activityDefinitionId");
            }
            if (exception == null)
            {
                throw FxTrace.Exception.ArgumentNull("exception");
            }
            if (faultSource == null)
            {
                throw FxTrace.Exception.ArgumentNull("faultSource");
            }
            this.FaultSource = faultSource;
            this.UnhandledException = exception;
            this.Level = TraceLevel.Error;
        }

        public WorkflowInstanceUnhandledExceptionRecord(Guid instanceId, string activityDefinitionId, ActivityInfo faultSource, Exception exception, WorkflowIdentity workflowDefinitionIdentity)
            : this(instanceId, activityDefinitionId, faultSource, exception)
        {
            this.WorkflowDefinitionIdentity = workflowDefinitionIdentity;
        }

        public WorkflowInstanceUnhandledExceptionRecord(Guid instanceId, long recordNumber, string activityDefinitionId, ActivityInfo faultSource, Exception exception, WorkflowIdentity workflowDefinitionIdentity)
            : this(instanceId, recordNumber, activityDefinitionId, faultSource, exception)
        {
            this.WorkflowDefinitionIdentity = workflowDefinitionIdentity;
        }

        WorkflowInstanceUnhandledExceptionRecord(WorkflowInstanceUnhandledExceptionRecord record)
            : base(record)
        {
            this.FaultSource = record.FaultSource;
            this.UnhandledException = record.UnhandledException;
        }
        
        public Exception UnhandledException
        {
            get
            {
                return this.unhandledException;
            }
            private set
            {
                this.unhandledException = value;
            }
        }
        
        public ActivityInfo FaultSource
        {
            get
            {
                return this.faultSource;
            }
            private set
            {
                this.faultSource = value;
            }
        }

        [DataMember(Name = "UnhandledException")]
        internal Exception SerializedUnhandledException
        {
            get { return this.UnhandledException; }
            set { this.UnhandledException = value; }
        }

        [DataMember(Name = "FaultSource")]
        internal ActivityInfo SerializedFaultSource
        {
            get { return this.FaultSource; }
            set { this.FaultSource = value; }
        }

        protected internal override TrackingRecord Clone()
        {
            return new WorkflowInstanceUnhandledExceptionRecord(this);
        }

        public override string ToString()
        {
            // For backward compatibility, the ToString() does not return 
            // WorkflowIdentity, if it is null.
            if (this.WorkflowDefinitionIdentity == null)
            {
                return string.Format(CultureInfo.CurrentCulture,
                    "WorkflowInstanceUnhandledExceptionRecord {{ InstanceId = {0}, RecordNumber = {1}, EventTime = {2}, ActivityDefinitionId = {3}, FaultSource {{ {4} }}, UnhandledException = {5} }} ",
                    this.InstanceId,
                    this.RecordNumber,
                    this.EventTime,
                    this.ActivityDefinitionId,
                    this.FaultSource.ToString(),
                    this.UnhandledException);
            }
            else
            {
                return string.Format(CultureInfo.CurrentCulture,
                    "WorkflowInstanceUnhandledExceptionRecord {{ InstanceId = {0}, RecordNumber = {1}, EventTime = {2}, ActivityDefinitionId = {3}, FaultSource {{ {4} }}, UnhandledException = {5}, WorkflowDefinitionIdentity = {6} }} ",
                    this.InstanceId,
                    this.RecordNumber,
                    this.EventTime,
                    this.ActivityDefinitionId,
                    this.FaultSource.ToString(),
                    this.UnhandledException,
                    this.WorkflowDefinitionIdentity);
            }
        }
    }
}
