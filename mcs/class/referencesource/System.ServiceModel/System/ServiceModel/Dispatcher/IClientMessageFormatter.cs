//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel.Channels;
    using System.Collections;

    public interface IClientMessageFormatter
    {
        Message SerializeRequest(MessageVersion messageVersion, object[] parameters);
        object DeserializeReply(Message message, object[] parameters);
    }
}
