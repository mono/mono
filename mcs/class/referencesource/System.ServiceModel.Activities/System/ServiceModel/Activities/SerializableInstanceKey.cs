//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.Runtime.Serialization;
    using System.Xml.Linq;

    [DataContract(Name = "InstanceKey", Namespace = XD2.DurableInstancing.Namespace)]
    class SerializableInstanceKey
    {
        Guid value;
        IDictionary<XName, SerializableInstanceValue> metadata;

        public SerializableInstanceKey(InstanceKey instanceKey)
        {
            this.value = instanceKey.Value;
            if (instanceKey.Metadata != null)
            {
                this.metadata = new Dictionary<XName, SerializableInstanceValue>(instanceKey.Metadata.Count);
                foreach (KeyValuePair<XName, InstanceValue> pair in instanceKey.Metadata)
                {
                    this.metadata.Add(pair.Key, new SerializableInstanceValue(pair.Value));
                }
            }
        }

        [DataMember(EmitDefaultValue = false)]
        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode, Justification = "Called from Serialization")]
        internal Guid Value
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

        [DataMember(EmitDefaultValue = false)]
        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode, Justification = "Called from Serialization")]
        internal IDictionary<XName, SerializableInstanceValue> Metadata
        {
            get
            {
                if (this.metadata == null || this.metadata.Count == 0)
                {
                    return null;
                }
                else
                {
                    return this.metadata;
                }
            }

            set
            {
                this.metadata = value;
            }
        }

        public InstanceKey ToInstanceKey()
        {
            IDictionary<XName, InstanceValue> metadata = null;
            if (this.metadata != null)
            {
                metadata = new Dictionary<XName, InstanceValue>(this.metadata.Count);
                foreach (KeyValuePair<XName, SerializableInstanceValue> pair in this.metadata)
                {
                    metadata.Add(pair.Key, pair.Value.ToInstanceValue());
                }
            }

            return new InstanceKey(this.value, metadata);
        }
    }
}
