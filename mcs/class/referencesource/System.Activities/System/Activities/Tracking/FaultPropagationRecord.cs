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
    public sealed class FaultPropagationRecord : TrackingRecord
    {
        ActivityInfo faultSource;
        ActivityInfo faultHandler;
        bool isFaultSource;
        Exception fault;
        
        internal FaultPropagationRecord(Guid instanceId, ActivityInstance source, ActivityInstance faultHandler, bool isFaultSource, Exception fault)
            : base(instanceId)
        {
            Fx.Assert(source != null, "Fault source cannot be null");
            this.FaultSource = new ActivityInfo(source);

            if (faultHandler != null)
            {
                this.FaultHandler = new ActivityInfo(faultHandler);
            }
            this.IsFaultSource = isFaultSource;
            this.Fault = fault;
            this.Level = TraceLevel.Warning;
        }

        //parameter faultHandler is null if there are no handlers
        public FaultPropagationRecord(
             Guid instanceId,
             long recordNumber,
             ActivityInfo faultSource,
             ActivityInfo faultHandler,
             bool isFaultSource,
             Exception fault)
            : base(instanceId, recordNumber)
        {
            if (faultSource == null)
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("faultSource");
            }

            this.FaultSource = faultSource;
            this.FaultHandler = faultHandler;
            this.IsFaultSource = isFaultSource;
            this.Fault = fault;
            this.Level = TraceLevel.Warning;
        }

        FaultPropagationRecord(FaultPropagationRecord record)
            : base(record)
        {
            this.FaultSource = record.FaultSource;
            this.FaultHandler = record.FaultHandler;
            this.Fault = record.Fault;
            this.IsFaultSource = record.IsFaultSource;
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
                
        public ActivityInfo FaultHandler
        {
            get
            {
                return this.faultHandler;
            }
            private set
            {
                this.faultHandler = value;
            }
        }
        
        public bool IsFaultSource
        {
            get
            {
                return this.isFaultSource;
            }
            private set
            {
                this.isFaultSource = value;
            }
        }
        
        public Exception Fault
        {
            get
            {
                return this.fault;
            }
            private set
            {
                this.fault = value;
            }
        }

        [DataMember(Name = "FaultSource")]
        internal ActivityInfo SerializedFaultSource
        {
            get { return this.FaultSource; }
            set { this.FaultSource = value; }
        }

        [DataMember(Name = "FaultHandler")]
        internal ActivityInfo SerializedFaultHandler
        {
            get { return this.FaultHandler; }
            set { this.FaultHandler = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "IsFaultSource")]
        internal bool SerializedIsFaultSource
        {
            get { return this.IsFaultSource; }
            set { this.IsFaultSource = value; }
        }

        [DataMember(Name = "Fault")]
        internal Exception SerializedFault
        {
            get { return this.Fault; }
            set { this.Fault = value; }
        }

        protected internal override TrackingRecord Clone()
        {
            return new FaultPropagationRecord(this);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture,
                "FaultPropagationRecord {{ {0}, FaultSource {{ {1} }}, FaultHandler {{ {2} }}, IsFaultSource = {3} }}",
                base.ToString(),
                this.FaultSource.ToString(),
                this.FaultHandler != null ? this.FaultHandler.ToString() : "<null>",
                this.IsFaultSource);
        }

    }
}
