// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Channels
{
    using System.Net.WebSockets;

    public interface IWebSocketCloseDetails
    {
        WebSocketCloseStatus? InputCloseStatus { get; }

        string InputCloseStatusDescription { get; }

        void SetOutputCloseStatus(WebSocketCloseStatus closeStatus, string closeStatusDescription);
    }
}
