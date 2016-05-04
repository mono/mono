//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ServiceModel.Channels;

    public abstract class MessageQueryCollection : Collection<MessageQuery>
    {
        public abstract IEnumerable<KeyValuePair<MessageQuery, TResult>> Evaluate<TResult>(Message message);
        public abstract IEnumerable<KeyValuePair<MessageQuery, TResult>> Evaluate<TResult>(MessageBuffer buffer);
    }
}
