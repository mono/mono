//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    
    // These are information passed from Receive to Send
    class CorrelationResponseContext 
    {
        internal WorkflowOperationContext WorkflowOperationContext
        {
            get;
            set;
        }

        internal Exception Exception
        {
            get;
            set;
        }

        // Used by the ToReply formatter
        internal MessageVersion MessageVersion
        {
            get;
            set;
        }
    }
}
