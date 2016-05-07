// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Channels
{
    using System.Collections.ObjectModel;
    using System.Net.Http;
    using System.Net.WebSockets;

    public sealed class WebSocketMessageProperty
    {
        public const string Name = "WebSocketMessageProperty";
        WebSocketContext context;
        string subProtocol;
        WebSocketMessageType messageType;
        ReadOnlyDictionary<string, object> properties;

        public WebSocketMessageProperty()
        {
            this.messageType = WebSocketDefaults.DefaultWebSocketMessageType;
        }

        internal WebSocketMessageProperty(WebSocketContext context, string subProtocol, WebSocketMessageType incomingMessageType, ReadOnlyDictionary<string, object> properties)
        {
            this.context = context;
            this.subProtocol = subProtocol;
            this.messageType = incomingMessageType;
            this.properties = properties;
        }

        public WebSocketContext WebSocketContext
        {
            get { return this.context; }
        }

        public string SubProtocol
        {
            get { return this.subProtocol; }
        }

        public WebSocketMessageType MessageType
        {
            get { return this.messageType; }
            set { this.messageType = value; }
        }

        public ReadOnlyDictionary<string, object> OpeningHandshakeProperties
        {
            get 
            {
                if (this.properties == null)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.GetString(
                        SR.WebSocketOpeningHandshakePropertiesNotAvailable,
                        "RequestMessage",
                        typeof(HttpResponseMessage).Name,
                        typeof(DelegatingHandler).Name)));
                }

                return this.properties; 
            }
        }
    }
}
