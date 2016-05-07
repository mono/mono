//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{

    public sealed class ContextCorrelationInitializer : CorrelationInitializer
    {
        public ContextCorrelationInitializer()
            : base()
        {
        }

        internal override CorrelationInitializer CloneCore()
        {
            return new ContextCorrelationInitializer();
        }
    }
}
