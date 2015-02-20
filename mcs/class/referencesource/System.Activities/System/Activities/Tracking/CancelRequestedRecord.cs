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
    public sealed class CancelRequestedRecord : TrackingRecord
    {
        ActivityInfo activity;
        ActivityInfo child;

        internal CancelRequestedRecord(Guid instanceId, ActivityInstance instance, ActivityInstance child)
            : base(instanceId)
        {
            Fx.Assert(child != null, "Child activity instance cannot be null.");
            if (instance != null)
            {
                this.Activity = new ActivityInfo(instance);
            }
            this.Child = new ActivityInfo(child);            
        }

        //parameter activity is null if the root activity is being cancelled.
        public CancelRequestedRecord(
            Guid instanceId,
            long recordNumber,
            ActivityInfo activity,
            ActivityInfo child)
            : base(instanceId, recordNumber)
        {
            if (child == null)
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("child");
            }

            this.Activity = activity;
            this.Child = child;
        }

        CancelRequestedRecord(CancelRequestedRecord record)
            : base(record)
        {
            this.Activity = record.Activity;
            this.Child = record.Child;            
        }
        
        public ActivityInfo Activity
        {
            get
            {
                return this.activity;
            }
            private set
            {
                this.activity = value;
            }
        }
        
        public ActivityInfo Child
        {
            get
            {
                return this.child;
            }
            private set
            {
                this.child = value;
            }
        }

        [DataMember(Name = "Activity")]
        internal ActivityInfo SerializedActivity
        {
            get { return this.Activity; }
            set { this.Activity = value; }
        }

        [DataMember(Name = "Child")]
        internal ActivityInfo SerializedChild
        {
            get { return this.Child; }
            set { this.Child = value; }
        }

        protected internal override TrackingRecord Clone()
        {
            return new CancelRequestedRecord(this);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture,
               "CancelRequestedRecord {{ {0}, Activity {{ {1} }}, ChildActivity {{ {2} }} }}",
               base.ToString(),
               this.Activity != null ? this.Activity.ToString() : "<null>",
               this.Child.ToString());
        }

    }
}
