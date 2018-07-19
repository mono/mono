//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Activities;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;

    public abstract class CorrelationInitializer
    {
        // internal ctor since we control the hierarchy
        // only supported subclasses are RequestReplyCorrelationInitializer, QueryCorrelationInitializer,
        // CallbackCorrelationInitializer and ContextCorrelationInitializer
        internal CorrelationInitializer()
        {
        }

        [DefaultValue(null)]
        public InArgument<CorrelationHandle> CorrelationHandle
        {
            get;
            set;
        }

        internal string ArgumentName
        {
            get;
            set;
        }

        internal abstract CorrelationInitializer CloneCore();

        internal CorrelationInitializer Clone()
        {
            CorrelationInitializer clone = CloneCore();

            if (this.CorrelationHandle != null)
            {
                clone.CorrelationHandle = (InArgument<CorrelationHandle>)InArgument.CreateReference(this.CorrelationHandle, this.ArgumentName);
            }

            return clone;
        }
    }
}
