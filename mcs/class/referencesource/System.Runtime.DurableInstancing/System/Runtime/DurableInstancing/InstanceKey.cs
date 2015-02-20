//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Runtime.DurableInstancing
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;
    using System.Xml.Linq;

    [Fx.Tag.XamlVisible(false)]
    [DataContract]
    public class InstanceKey
    {
        static IDictionary<XName, InstanceValue> emptyMetadata = new ReadOnlyDictionaryInternal<XName, InstanceValue>(new Dictionary<XName, InstanceValue>(0));
        static InstanceKey invalidKey = new InstanceKey();

        bool invalid; // Comparisons to Guid.Empty are too slow.
        IDictionary<XName, InstanceValue> metadata;

        InstanceKey()
        {
            this.Value = Guid.Empty;
            this.invalid = true;
        }

        public InstanceKey(Guid value)
            : this(value, null)
        {
        }

        public InstanceKey(Guid value, IDictionary<XName, InstanceValue> metadata)
        {
            if (value == Guid.Empty)
            {
                throw Fx.Exception.Argument("value", SRCore.InstanceKeyRequiresValidGuid);
            }

            this.Value = value;
            if (metadata != null)
            {
                if (metadata.IsReadOnly)
                {
                    this.Metadata = metadata;
                }
                else
                {
                    Dictionary<XName, InstanceValue> copy = new Dictionary<XName, InstanceValue>(metadata);
                    this.Metadata = new ReadOnlyDictionaryInternal<XName, InstanceValue>(copy);
                }

            }
            else
            {
                this.Metadata = emptyMetadata;
            }
        }

        public bool IsValid
        {
            get
            {
                return !this.invalid;
            }
        }

        public Guid Value
        {
            get;
            private set;
        }

        public IDictionary<XName, InstanceValue> Metadata
        {
            get
            {
                // This can be true if the object was deserialized.
                if (this.metadata == null)
                {
                    this.metadata = emptyMetadata;
                }
                return this.metadata;
            }
            private set
            {
                this.metadata = value;
            }
        }

        public static InstanceKey InvalidKey
        {
            get
            {
                return InstanceKey.invalidKey;
            }
        }

        public override bool Equals(object obj)
        {
            return this.Value.Equals(((InstanceKey)obj).Value);
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        [DataMember(Name = "Value")]
        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode, Justification = "Called from Serialization")]
        internal Guid SerializedValue
        {
            get
            {
                return this.Value;
            }
            set
            {
                if (value.CompareTo(Guid.Empty) == 0)
                {
                    this.invalid = true;
                }
                else
                {
                    this.Value = value;
                }
            }
        }

        [DataMember(Name = "Metadata", EmitDefaultValue = false)]
        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode, Justification = "Called from Serialization")]
        internal IDictionary<XName, InstanceValue> SerializedMetadata
        {
            get
            {
                if (this.Metadata.Count == 0)
                {
                    return null;
                }
                else
                {
                    return this.Metadata;
                }
            }
            set
            {
                Fx.Assert(value != null, "A null value should not have been serialized because EmitDefaultValue is false");
                this.Metadata = value;
            }
        }
    }
}
