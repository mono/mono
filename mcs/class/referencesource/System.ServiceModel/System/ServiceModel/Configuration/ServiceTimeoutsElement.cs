//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.ComponentModel;
    using System.Configuration;
    using System.Runtime;
    using System.ServiceModel.Description;

    public sealed partial class ServiceTimeoutsElement : BehaviorExtensionElement
    {
        public ServiceTimeoutsElement()
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

            ServiceTimeoutsElement source = (ServiceTimeoutsElement)from;
#pragma warning suppress 56506 //[....]; base.CopyFrom() checks for 'from' being null
            this.TransactionTimeout = source.TransactionTimeout;
        }

        protected internal override object CreateBehavior()
        {
            return new ServiceTimeoutsBehavior(this.TransactionTimeout);
        }

        public override Type BehaviorType
        {
            get { return typeof(ServiceTimeoutsBehavior); }
        }
    }
}

