//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.Activation
{
    using System;
    using System.Net.Sockets;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Runtime.Serialization;

    interface IConnectionDuplicator
    {
        [OperationContract(IsOneWay = false, AsyncPattern = true)]
        IAsyncResult BeginDuplicate(
            DuplicateContext duplicateContext,
            AsyncCallback callback, object state);

        void EndDuplicate(IAsyncResult result);
    }
}
