//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Tracking
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Runtime.Diagnostics;

    [Fx.Tag.XamlVisible(false)]
    [DataContract]
    public abstract class TrackingRecord
    {
        IDictionary<string, string> annotations;
        DateTime eventTime;
        TraceLevel level;
        EventTraceActivity eventTraceActivity;

        static ReadOnlyDictionaryInternal<string, string> readonlyEmptyAnnotations;

        protected TrackingRecord(Guid instanceId)
        {
            this.InstanceId = instanceId;
            this.EventTime = DateTime.UtcNow;
            this.Level = TraceLevel.Info;
            this.eventTraceActivity = new EventTraceActivity(instanceId);
        }

        protected TrackingRecord(Guid instanceId, long recordNumber)
            : this(instanceId)
        {
            this.RecordNumber = recordNumber;
        }

        protected TrackingRecord(TrackingRecord record)
        {
            this.InstanceId = record.InstanceId;
            this.RecordNumber = record.RecordNumber;
            this.EventTime = record.EventTime;
            this.Level = record.Level;
            if (record.HasAnnotations)
            {
                Dictionary<string, string> copy = new Dictionary<string, string>(record.annotations);
                this.annotations = new ReadOnlyDictionaryInternal<string, string>(copy);
            }
        }


        [DataMember]
        public Guid InstanceId
        {
            get;
            internal set;
        }

        [DataMember]
        public long RecordNumber
        {
            get;
            internal set;
        }

        public DateTime EventTime
        {
            get
            {
                return this.eventTime;
            }
            private set
            {
                this.eventTime = value;
            }
        }

        public TraceLevel Level
        {
            get
            {
                return this.level;
            }
            protected set
            {
                this.level = value;
            }
        }

        public IDictionary<string, string> Annotations
        {
            get
            {
                if (this.annotations == null)
                {
                    this.annotations = ReadOnlyEmptyAnnotations;
                }
                return this.annotations;
            }
            internal set
            {
                Fx.Assert(value.IsReadOnly, "only readonly dictionary can be set for annotations");
                this.annotations = value;
            }
        }

        [DataMember(EmitDefaultValue = false, Name = "annotations")]
        internal IDictionary<string, string> SerializedAnnotations
        {
            get { return this.annotations; }
            set { this.annotations = value; }
        }

        [DataMember(Name = "EventTime")]
        internal DateTime SerializedEventTime
        {
            get { return this.EventTime; }
            set { this.EventTime = value; }
        }

        [DataMember(Name = "Level")]
        internal TraceLevel SerializedLevel
        {
            get { return this.Level; }
            set { this.Level = value; }
        }

        internal EventTraceActivity EventTraceActivity
        {
            get
            {
                if (this.eventTraceActivity == null)
                {
                    this.eventTraceActivity = new EventTraceActivity(this.InstanceId);
                }

                return this.eventTraceActivity;
            }
        }

        static ReadOnlyDictionaryInternal<string, string> ReadOnlyEmptyAnnotations
        {
            get
            {
                if (readonlyEmptyAnnotations == null)
                {
                    readonlyEmptyAnnotations = new ReadOnlyDictionaryInternal<string, string>(new Dictionary<string, string>(0));
                }
                return readonlyEmptyAnnotations;
            }
        }

        internal bool HasAnnotations
        {
            get
            {
                return (this.annotations != null && this.annotations.Count > 0);
            }
        }

        protected abstract internal TrackingRecord Clone();

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture,
                "InstanceId = {0}, RecordNumber = {1}, EventTime = {2}",
                this.InstanceId,
                this.RecordNumber,
                this.EventTime);
        }
    }
}
