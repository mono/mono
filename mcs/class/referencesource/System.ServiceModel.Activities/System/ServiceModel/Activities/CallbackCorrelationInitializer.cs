//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{

    public sealed class CallbackCorrelationInitializer : CorrelationInitializer
    {
        public CallbackCorrelationInitializer()
            : base()
        {
        }

        internal override CorrelationInitializer CloneCore()
        {
            return new CallbackCorrelationInitializer();
        }
    }
}
