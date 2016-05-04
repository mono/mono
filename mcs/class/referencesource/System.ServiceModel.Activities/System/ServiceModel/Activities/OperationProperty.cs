//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel.Description;
    
    class OperationProperty
    {
        HashSet<Receive> implementingReceives;
        HashSet<Receive> implementingSendRepliesRequests;

        public OperationProperty(OperationDescription operation)
        {
            this.Operation = operation;
        }

        public OperationDescription Operation { get; private set; }

        public HashSet<Receive> ImplementingReceives
        {
            get
            {
                if (this.implementingReceives == null)
                {
                    // HashSet item type, Receive, is sealed, so we know we don't need explicit object reference EqualityComparer here
                    this.implementingReceives = new HashSet<Receive>();
                }
                return this.implementingReceives;
            }
        }

        // this is a set of Request property values of SendReply activities
        public HashSet<Receive> ImplementingSendRepliesRequests
        {
            get
            {
                if (this.implementingSendRepliesRequests == null)
                {
                    this.implementingSendRepliesRequests = new HashSet<Receive>();
                }
                return this.implementingSendRepliesRequests;
            }
        }
    }
}
