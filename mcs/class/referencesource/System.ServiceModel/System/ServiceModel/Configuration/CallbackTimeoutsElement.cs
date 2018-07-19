//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.ComponentModel;
    using System.Configuration;
    using System.Runtime;
    using System.ServiceModel.Description;

    public sealed partial class CallbackTimeoutsElement : BehaviorExtensionElement
    {
        public CallbackTimeoutsElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.TransactionTimeout, DefaultValue = ServiceDefaults.TransactionTimeoutString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan TransactionTimeout
        {
            get { return (TimeSpan)base[ConfigurationStrings.TransactionTimeout]; }
            set { base[ConfigurationStrings.TransactionTimeout] = value; }
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);

            CallbackTimeoutsElement source = (CallbackTimeoutsElement)from;
#pragma warning suppress 56506 //Microsoft; base.CopyFrom() checks for 'from' being null
            this.TransactionTimeout = source.TransactionTimeout;
        }

        protected internal override object CreateBehavior()
        {
            CallbackTimeoutsBehavior behavior = new CallbackTimeoutsBehavior();
            behavior.TransactionTimeout = this.TransactionTimeout;
            return behavior;
        }

        public override Type BehaviorType
        {
            get { return typeof(CallbackTimeoutsBehavior); }
        }
    }
}

