//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Collections.Generic;

    interface IUdpReceiveHandler
    {
        int MaxReceivedMessageSize { get; }
        void HandleAsyncException(Exception exception);

        //returns false if the message was dropped because the max pending message count was hit.
        bool HandleDataReceived(ArraySegment<byte> data, EndPoint remoteEndpoint, int interfaceIndex, Action onMessageDequeuedCallback);
    }

}
