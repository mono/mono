//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel.Description;

    public sealed partial class CallbackDebugElement : BehaviorExtensionElement
    {
        public CallbackDebugElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.IncludeExceptionDetailInFaults, DefaultValue = false)]
        public bool IncludeExceptionDetailInFaults
        {
            get { return (bool)base[ConfigurationStrings.IncludeExceptionDetailInFaults]; }
            set { base[ConfigurationStrings.IncludeExceptionDetailInFaults] = value; }
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);

            CallbackDebugElement source = (CallbackDebugElement)from;
#pragma warning suppress 56506 //[....]; base.CopyFrom() check for 'from' being null
            this.IncludeExceptionDetailInFaults = source.IncludeExceptionDetailInFaults;
        }

        protected internal override object CreateBehavior()
        {
            return new CallbackDebugBehavior(this.IncludeExceptionDetailInFaults);
        }

        public override Type BehaviorType
        {
            get { return typeof(CallbackDebugBehavior); }
        }
    }
}
