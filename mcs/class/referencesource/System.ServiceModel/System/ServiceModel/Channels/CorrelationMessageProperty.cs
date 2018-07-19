//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.Runtime.Serialization;

    [DataContract]
    public class CorrelationMessageProperty
    {
        static readonly ReadOnlyCollection<InstanceKey> emptyInstanceKeyList = new ReadOnlyCollection<InstanceKey>(new List<InstanceKey>(0));

        const string PropertyName = "CorrelationMessageProperty";
        ReadOnlyCollection<InstanceKey> additionalKeys;
        InstanceKey correlationKey;
        ReadOnlyCollection<InstanceKey> transientCorrelations;

        public CorrelationMessageProperty(InstanceKey correlationKey, IEnumerable<InstanceKey> additionalKeys)
            : this(correlationKey, additionalKeys, null)
        {
        }

        public CorrelationMessageProperty(InstanceKey correlationKey, IEnumerable<InstanceKey> additionalKeys, IEnumerable<InstanceKey> transientCorrelations)
        {
            if (correlationKey == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("correlationKey");
            }

            if (additionalKeys == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("additionalKeys");
            }

            this.correlationKey = correlationKey;

            ICollection<InstanceKey> additionalKeysCollection = additionalKeys as ICollection<InstanceKey>;
            if (additionalKeysCollection != null && additionalKeysCollection.Count == 0)
            {
                this.additionalKeys = emptyInstanceKeyList;
            }
            else
            {
                this.additionalKeys = additionalKeys as ReadOnlyCollection<InstanceKey>;
                if (this.additionalKeys == null)
                {
                    IList<InstanceKey> additionalKeysList = additionalKeys as IList<InstanceKey>;
                    if (additionalKeysList == null)
                    {
                        additionalKeysList = new List<InstanceKey>(additionalKeys);
                    }
                    this.additionalKeys = new ReadOnlyCollection<InstanceKey>(additionalKeysList);
                }
            }

            ICollection<InstanceKey> transientCorrelationsCollection = transientCorrelations as ICollection<InstanceKey>;
            if (transientCorrelations == null || (transientCorrelationsCollection != null && transientCorrelationsCollection.Count == 0))
            {
                this.transientCorrelations = emptyInstanceKeyList;
            }
            else
            {
                this.transientCorrelations = transientCorrelations as ReadOnlyCollection<InstanceKey>;
                if (this.transientCorrelations == null)
                {
                    IList<InstanceKey> transientCorrelationsList = transientCorrelations as IList<InstanceKey>;
                    if (transientCorrelationsList == null)
                    {
                        transientCorrelationsList = new List<InstanceKey>(transientCorrelations);
                    }
                    this.transientCorrelations = new ReadOnlyCollection<InstanceKey>(transientCorrelationsList);
                }
            }
        }

        public static string Name
        {
            get { return PropertyName; }
        }

        public InstanceKey CorrelationKey
        {
            get { return this.correlationKey; }
        }

        public ReadOnlyCollection<InstanceKey> AdditionalKeys
        {
            get 
            { 
                // This can be true if the object was deserialized.
                if (this.additionalKeys == null)
                {
                    this.additionalKeys = emptyInstanceKeyList;
                }
                return this.additionalKeys; 
            }
        }

        public ReadOnlyCollection<InstanceKey> TransientCorrelations
        {
            get 
            { 
                // This can be true if the object was deserialized.
                if (this.transientCorrelations == null)
                {
                    this.transientCorrelations = emptyInstanceKeyList;
                }
                return this.transientCorrelations; 
            }
        }

        public static bool TryGet(Message message, out CorrelationMessageProperty property)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            return TryGet(message.Properties, out property);
        }

        public static bool TryGet(MessageProperties properties, out CorrelationMessageProperty property)
        {
            if (properties == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("properties");
            }

            object value = null;
            if (properties.TryGetValue(PropertyName, out value))
            {
                property = value as CorrelationMessageProperty;
            }
            else
            {
                property = null;
            }
            return property != null;
        }

        // Surrogate for serialization purposes
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used by serialization")]
        [DataMember(Name = "CorrelationKey", EmitDefaultValue = false)]
        internal InstanceKey SerializedCorrelationKey
        {
            get
            {
                return this.correlationKey;
            }

            set
            {
                this.correlationKey = value;
            }
        }

        // Surrogate for serialization purposes
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used by serialization")]
        [DataMember(Name = "AdditionalCorrelations", EmitDefaultValue = false)]
        internal List<InstanceKey> SerializedAdditionalKeys
        {
            get
            {
                if (this.AdditionalKeys.Count == 0)
                {
                    return null;
                }
                return new List<InstanceKey>(this.AdditionalKeys);
            }

            set
            {
                Fx.Assert(value != null, "A null value should not have been serialized because EmitDefaultValue is false");
                this.additionalKeys = new ReadOnlyCollection<InstanceKey>(value);
            }
        }

        // Surrogate for serialization purposes
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used by serialization")]
        [DataMember(Name = "TransientCorrelations", EmitDefaultValue = false)]
        internal List<InstanceKey> SerializedTransientCorrelations
        {
            get
            {
                if (this.TransientCorrelations.Count == 0)
                {
                    return null;
                }
                return new List<InstanceKey>(this.TransientCorrelations);
            }

            set
            {
                Fx.Assert(value != null, "A null value should not have been serialized because EmitDefaultValue is false");
                this.transientCorrelations = new ReadOnlyCollection<InstanceKey>(value);
            }
        }
    }
}
