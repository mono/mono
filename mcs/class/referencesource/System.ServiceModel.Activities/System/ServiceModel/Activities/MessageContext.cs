//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel.Channels;

    public class MessageContext
    {
        Message message;
        Guid traceId;

        public MessageContext()
        {
        }

        internal MessageContext(Message message)
        {
            this.message = message;
        }

        public virtual Message Message
        {
            get
            {
                return this.message;
            }

            set
            {
                this.message = value;
            }
        }

        public virtual Guid EndToEndTracingId
        {
            get 
            { 
                return this.traceId; 
            }
            
            set 
            { 
                this.traceId = value; 
            }
        }
    }
}
