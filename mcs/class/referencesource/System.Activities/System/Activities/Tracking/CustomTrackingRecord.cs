//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Runtime.Serialization;
    using System.Runtime;
    using System.Diagnostics;
    using System.Globalization;

    [DataContract]
    [Fx.Tag.XamlVisible(false)]
    public class CustomTrackingRecord : TrackingRecord
    {
        IDictionary<string, object> data;
        string name;
        ActivityInfo activity;

        public CustomTrackingRecord(string name)
            : this(name, TraceLevel.Info)
        {
        }

        public CustomTrackingRecord(string name, TraceLevel level)
            : this(Guid.Empty, name, level)
        {
        }

        public CustomTrackingRecord(Guid instanceId, string name, TraceLevel level)
            : base(instanceId)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw FxTrace.Exception.ArgumentNull("name");
            }
            this.Name = name;
            this.Level = level;
        }

        protected CustomTrackingRecord(CustomTrackingRecord record)
            : base(record)
        {
            this.Name = record.Name;
            this.Activity = record.Activity;
            if (record.data != null && record.data.Count > 0)
            {
                foreach (KeyValuePair<string, object> item in record.data)
                {
                    this.Data.Add(item);
                }
            }
        }
        
        public string Name
        {
            get
            {
                return this.name;
            }
            private set
            {
                this.name = value;
            }
        }

        public ActivityInfo Activity
        {
            get { return this.activity; }
            internal set { this.activity = value; }
        }

        public IDictionary<string, object> Data
        {
            get
            {
                if (this.data == null)
                {
                    this.data = new Dictionary<string, object>();
                }
                return this.data;
            }
        }

        [DataMember(EmitDefaultValue = false, Name = "data")]
        internal IDictionary<string, object> SerializedData
        {
            get { return this.data; }
            set { this.data = value; }
        }

        [DataMember(Name = "Name")]
        internal string SerializedName
        {
            get { return this.Name; }
            set { this.Name = value; }
        }

        [DataMember(Name = "Activity")]
        internal ActivityInfo SerializedActivity
        {
            get { return this.Activity; }
            set { this.Activity = value; }
        }

        protected internal override TrackingRecord Clone()
        {
            return new CustomTrackingRecord(this);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "CustomTrackingRecord {{ {0}, Name={1}, Activity {{ {2} }}, Level = {3} }}",
                base.ToString(),
                this.Name,
                this.Activity == null ? "<null>" : this.Activity.ToString(),
                this.Level);
        }
    }
}
