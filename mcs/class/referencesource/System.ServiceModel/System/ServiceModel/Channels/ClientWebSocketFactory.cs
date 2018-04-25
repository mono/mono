// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Channels
{
    using System.IO;
    using System.Net.WebSockets;

    // In Win8 (and above), a client web socket can simply be created in 2 steps:
    // 1. create a HttpWebRequest with the Uri = "ws://server_address"
    // 2. create a client WebSocket with WebSocket.CreateClientWebSocket(stream_requested_from_the_HttpWebRequest)
    // On pre-Win8, the WebSocket.CreateClientWebSocket method doesn't work, so users needs to provide a factory for step #2.
    // WCF will internally create the HttpWebRequest from step #1 and will call the web socket factory for step #2.
    // A factory can also be used in Win8 (and above), if the user desires to use his own WebSocket implementation.
    public abstract class ClientWebSocketFactory
    {
        // Provides the web socket version, to be used as the required http header "Sec-WebSocket-Version".
        // When creating the HttpWebRequest from step #1, the web socket header is not initialized.
        public abstract string WebSocketVersion { get; }

        // Provides the client WebSocket for step #2. WCF creates the HttpWebRequest in step #1, and passes the HttpWebResponse stream
        // to this method. The 'settings' argument can optionally be used. On Win8 (and above), the WebSocket.CreateClientWebSocket method 
        // requires other arguments (in addition to the Stream) that can be obtained from 'settings'. Since the WebSocket.CreateClientWebSocket 
        // finds this argument to be enough to create a client WebSocket (on Win8, and post Win8 due to backward compatibility requirements), 
        // we estimate that implementors of a custom web socket factory will find it enough too.
        public abstract WebSocket CreateWebSocket(Stream connection, WebSocketTransportSettings settings);
    }
}