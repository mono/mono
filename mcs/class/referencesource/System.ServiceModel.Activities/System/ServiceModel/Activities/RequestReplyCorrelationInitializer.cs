//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{

    public sealed class RequestReplyCorrelationInitializer : CorrelationInitializer
    {
        public RequestReplyCorrelationInitializer()
            : base()
        {
        }

        internal override CorrelationInitializer CloneCore()
        {
            return new RequestReplyCorrelationInitializer();
        }
    }
}
