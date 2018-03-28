//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System.ServiceModel.Channels;

    public abstract class MessageQuery
    {
        public virtual MessageQueryCollection CreateMessageQueryCollection()
        {
            return null;
        }

        public abstract TResult Evaluate<TResult>(Message message);
        public abstract TResult Evaluate<TResult>(MessageBuffer buffer);
    }
}

