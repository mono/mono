//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Runtime.DurableInstancing
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Runtime.Serialization;

    [Fx.Tag.XamlVisible(false)]
    [DataContract]
    public sealed class InstanceValue
    {
        readonly static InstanceValue deletedValue = new InstanceValue();

        public InstanceValue(object value)
            : this(value, InstanceValueOptions.None)
        {
        }

        public InstanceValue(object value, InstanceValueOptions options)
        {
            Value = value;
            Options = options;
        }

        InstanceValue()
        {
            Value = this;
        }

        public object Value { get; private set; }

        public InstanceValueOptions Options { get; private set; }

        public bool IsDeletedValue
        {
            get
            {
                return object.ReferenceEquals(this, InstanceValue.DeletedValue);
            }
        }

        public static InstanceValue DeletedValue
        {
            get
            {
                return InstanceValue.deletedValue;
            }
        }

        [DataMember(Name = "Value", EmitDefaultValue = false)]
        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode, Justification = "Called from Serialization")]
        internal object SerializedValue
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

        [DataMember(Name = "Options", EmitDefaultValue = false)]
        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode, Justification = "Called from Serialization")]
        internal InstanceValueOptions SerializedOptions
        {
            get
            {
                return this.Options;
            }
            set
            {
                this.Options = value;
            }
        }
    }
}
