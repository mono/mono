//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Activities.Runtime;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using System.Runtime;
    using System.Collections.Specialized;

    [DataContract]
    [DebuggerDisplay("{Value}")]
    public abstract class Location
    {
        TemporaryResolutionData temporaryResolutionData;

        protected Location()
        {
        }

        public abstract Type LocationType
        {
            get;
        }

        public object Value
        {
            get
            {
                return this.ValueCore;
            }
            set
            {
                this.ValueCore = value;
            }
        }

        [DataMember(EmitDefaultValue = false, Name = "temporaryResolutionData")]
        internal TemporaryResolutionData SerializedTemporaryResolutionData
        {
            get { return this.temporaryResolutionData; }
            set { this.temporaryResolutionData = value; }
        }

        internal virtual bool CanBeMapped
        {
            get
            {
                return false;
            }
        }

        // When we are resolving an expression that resolves to a
        // reference to a location we need some way of notifying the
        // LocationEnvironment that it should extract the inner location
        // and throw away the outer one.  OutArgument and InOutArgument
        // create these TemporaryResolutionLocations if their expression
        // resolution goes async and LocationEnvironment gets rid of them
        // in CollapseTemporaryResolutionLocations().
        internal LocationEnvironment TemporaryResolutionEnvironment
        {
            get
            {
                return this.temporaryResolutionData.TemporaryResolutionEnvironment;
            }
        }

        internal bool BufferGetsOnCollapse
        {
            get
            {
                return this.temporaryResolutionData.BufferGetsOnCollapse;
            }
        }

        protected abstract object ValueCore
        {
            get;
            set;
        }

        internal void SetTemporaryResolutionData(LocationEnvironment resolutionEnvironment, bool bufferGetsOnCollapse)
        {
            this.temporaryResolutionData = new TemporaryResolutionData
            {
                TemporaryResolutionEnvironment = resolutionEnvironment,
                BufferGetsOnCollapse = bufferGetsOnCollapse
            };
        }

        internal virtual Location CreateReference(bool bufferGets)
        {
            if (this.CanBeMapped || bufferGets)
            {
                return new ReferenceLocation(this, bufferGets);
            }

            return this;
        }

        internal virtual object CreateDefaultValue()
        {
            Fx.Assert("We should only call this on Location<T>");
            return null;
        }

        [DataContract]
        internal struct TemporaryResolutionData
        {
            [DataMember(EmitDefaultValue = false)]
            public LocationEnvironment TemporaryResolutionEnvironment
            {
                get;
                set;
            }

            [DataMember(EmitDefaultValue = false)]
            public bool BufferGetsOnCollapse
            {
                get;
                set;
            }
        }

        [DataContract]
        internal class ReferenceLocation : Location
        {
            Location innerLocation;
            bool bufferGets;
            object bufferedValue;

            public ReferenceLocation(Location innerLocation, bool bufferGets)
            {
                this.innerLocation = innerLocation;
                this.bufferGets = bufferGets;
            }

            public override Type LocationType
            {
                get
                {
                    return this.innerLocation.LocationType;
                }
            }

            protected override object ValueCore
            {
                get
                {
                    if (this.bufferGets)
                    {
                        return this.bufferedValue;
                    }
                    else
                    {
                        return this.innerLocation.Value;
                    }
                }
                set
                {
                    this.innerLocation.Value = value;
                    this.bufferedValue = value;
                }
            }

            [DataMember(Name = "innerLocation")]
            internal Location SerializedInnerLocation
            {
                get { return this.innerLocation; }
                set { this.innerLocation = value; }
            }

            [DataMember(EmitDefaultValue = false, Name = "bufferGets")]
            internal bool SerializedBufferGets
            {
                get { return this.bufferGets; }
                set { this.bufferGets = value; }
            }

            [DataMember(EmitDefaultValue = false, Name = "bufferedValue")]
            internal object SerializedBufferedValue
            {
                get { return this.bufferedValue; }
                set { this.bufferedValue = value; }
            }

            public override string ToString()
            {
                if (bufferGets)
                {
                    return base.ToString();
                }
                else
                {
                    return this.innerLocation.ToString();
                }
            }
        }
    }

    [DataContract]
    public class Location<T> : Location
    {
        T value;

        public Location()
            : base()
        {
        }

        public override Type LocationType
        {
            get
            {
                return typeof(T);
            }
        }

        public virtual new T Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
            }
        }

        internal T TypedValue
        {
            get
            {
                return this.Value;
            }

            set
            {
                this.Value = value;
            }
        }

        protected override sealed object ValueCore
        {
            get
            {
                return this.Value;
            }

            set
            {
                this.Value = TypeHelper.Convert<T>(value);
            }
        }

        [DataMember(EmitDefaultValue = false, Name = "value")]
        internal T SerializedValue
        {
            get { return this.value; }
            set { this.value = value; }
        }

        internal override Location CreateReference(bool bufferGets)
        {
            if (this.CanBeMapped || bufferGets)
            {
                return new ReferenceLocation(this, bufferGets);
            }

            return this;
        }

        internal override object CreateDefaultValue()
        {
            Fx.Assert(typeof(T).GetGenericTypeDefinition() == typeof(Location<>), "We should only be calling this with location subclasses.");

            return Activator.CreateInstance<T>();
        }

        public override string ToString()
        {
            return this.value != null ? this.value.ToString() : "<null>";
        }

        [DataContract]
        internal new class ReferenceLocation : Location<T>
        {
            Location<T> innerLocation;
            bool bufferGets;

            public ReferenceLocation(Location<T> innerLocation, bool bufferGets)
            {
                this.innerLocation = innerLocation;
                this.bufferGets = bufferGets;
            }

            public override T Value
            {
                get
                {
                    if (this.bufferGets)
                    {
                        return this.value;
                    }
                    else
                    {
                        return this.innerLocation.Value;
                    }
                }
                set
                {
                    this.innerLocation.Value = value;

                    if (this.bufferGets)
                    {
                        this.value = value;
                    }
                }
            }

            [DataMember(Name = "innerLocation")]
            internal Location<T> SerializedInnerLocation
            {
                get { return this.innerLocation; }
                set { this.innerLocation = value; }
            }

            [DataMember(EmitDefaultValue = false, Name = "bufferGets")]
            internal bool SerializedBufferGets
            {
                get { return this.bufferGets; }
                set { this.bufferGets = value; }
            }

            public override string ToString()
            {
                if (this.bufferGets)
                {
                    return base.ToString();
                }
                else
                {
                    return this.innerLocation.ToString();
                }
            }
        }
    }
}
