//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.Runtime.Serialization;

    [DataContract(Name = "InstanceValue", Namespace = XD2.DurableInstancing.Namespace)]
    class SerializableInstanceValue
    {
        object value;
        int options;

        public SerializableInstanceValue(InstanceValue instanceValue)
        {
            this.value = instanceValue.Value;
            this.options = (int)instanceValue.Options;
        }

        [DataMember(EmitDefaultValue = false)]
        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode, Justification = "Called from Serialization")]
        internal object Value
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
        internal int Options
        {
            get
            {
                return this.options;
            }

            set
            {
                this.options = value;
            }
        }

        public InstanceValue ToInstanceValue()
        {
            return new InstanceValue(this.value, (InstanceValueOptions)this.options);
        }
    }
}
