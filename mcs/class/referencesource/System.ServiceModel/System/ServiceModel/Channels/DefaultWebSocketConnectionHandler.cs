// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.WebSockets;
    using System.Runtime;
    using System.Threading;

    class DefaultWebSocketConnectionHandler : WebSocketConnectionHandler
    {
        string currentVersion;
        string subProtocol;
        MessageEncoder encoder;
        string transferMode;
        bool needToCheckContentType;
        bool needToCheckTransferMode;
        Func<string, bool> checkVersionFunc;
        Func<string, bool> checkContentTypeFunc;
        Func<string, bool> checkTransferModeFunc;

        public DefaultWebSocketConnectionHandler(string subProtocol, string currentVersion, MessageVersion messageVersion, MessageEncoderFactory encoderFactory, TransferMode transferMode)
        {
            this.subProtocol = subProtocol;
            this.currentVersion = currentVersion;
            this.checkVersionFunc = new Func<string, bool>(this.CheckVersion);

            if (messageVersion != MessageVersion.None)
            {
                this.needToCheckContentType = true;
                this.encoder = encoderFactory.CreateSessionEncoder();
                this.checkContentTypeFunc = new Func<string, bool>(this.CheckContentType);

                if (encoderFactory is BinaryMessageEncoderFactory)
                {
                    this.needToCheckTransferMode = true;
                    this.transferMode = transferMode.ToString();
                    this.checkTransferModeFunc = new Func<string, bool>(this.CheckTransferMode);
                }
            }
        }

        protected internal override HttpResponseMessage AcceptWebSocket(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!CheckHttpHeader(request, WebSocketHelper.SecWebSocketVersion, this.checkVersionFunc))
            {
                return GetUpgradeRequiredResponseMessageWithVersion(request, this.currentVersion);
            }

            if (this.needToCheckContentType)
            {
                if (!CheckHttpHeader(request, WebSocketTransportSettings.SoapContentTypeHeader, this.checkContentTypeFunc))
                {
                    return this.GetBadRequestResponseMessageWithContentTypeAndTransfermode(request); 
                }

                if (this.needToCheckTransferMode && !CheckHttpHeader(request, WebSocketTransportSettings.BinaryEncoderTransferModeHeader, this.checkTransferModeFunc))
                {
                    return this.GetBadRequestResponseMessageWithContentTypeAndTransfermode(request);
                }
            }

            HttpResponseMessage response = GetWebSocketAcceptedResponseMessage(request);

            SubprotocolParseResult subprotocolParseResult = ParseSubprotocolValues(request);
            if (subprotocolParseResult.HeaderFound)
            {
                if (!subprotocolParseResult.HeaderValid)
                {
                    return GetBadRequestResponseMessage(request);
                }

                string negotiatedProtocol = null;

                // match client protocols vs server protocol
                foreach (string protocol in subprotocolParseResult.ParsedSubprotocols)
                {
                    if (string.Compare(protocol, this.subProtocol, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        negotiatedProtocol = protocol;
                        break;
                    }
                }

                if (negotiatedProtocol == null)
                {
                    FxTrace.Exception.AsWarning(new WebException(
                        SR.GetString(SR.WebSocketInvalidProtocolNotInClientList, this.subProtocol, string.Join(", ", subprotocolParseResult.ParsedSubprotocols))));

                    return GetUpgradeRequiredResponseMessageWithSubProtocol(request, this.subProtocol);
                }

                // set response header
                response.Headers.Remove(WebSocketHelper.SecWebSocketProtocol);
                if (negotiatedProtocol != string.Empty)
                {
                    response.Headers.Add(WebSocketHelper.SecWebSocketProtocol, negotiatedProtocol);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(this.subProtocol))
                {
                    FxTrace.Exception.AsWarning(new WebException(
                        SR.GetString(SR.WebSocketInvalidProtocolNoHeader, this.subProtocol, WebSocketHelper.SecWebSocketProtocol)));

                    return GetUpgradeRequiredResponseMessageWithSubProtocol(request, this.subProtocol);
                }
            }

            return response;
        }

        static SubprotocolParseResult ParseSubprotocolValues(HttpRequestMessage request)
        {
            Fx.Assert(request != null, "request should not be null");
            IEnumerable<string> clientProtocols = null;

            if (request.Headers.TryGetValues(WebSocketHelper.SecWebSocketProtocol, out clientProtocols))
            {
                List<string> tokenList = new List<string>();

                // We may have multiple subprotocol header in the response. We will build up a list with all the subprotocol values.
                // There might be duplicated ones inside the list, but it doesn't matter since we will always take the first matching value.
                foreach (string headerValue in clientProtocols)
                {
                    List<string> protocolList;
                    if (WebSocketHelper.TryParseSubProtocol(headerValue, out protocolList))
                    {
                        tokenList.AddRange(protocolList);
                    }
                    else
                    {
                        return SubprotocolParseResult.HeaderInvalid;
                    }
                }

                // If this method returns true, we should ensure that clientProtocols always contains at least one entry
                if (tokenList.Count == 0)
                {
                    tokenList.Add(string.Empty);
                }

                return new SubprotocolParseResult(true, true, tokenList);
            }

            return SubprotocolParseResult.HeaderNotFound;
        }

        static HttpResponseMessage GetUpgradeRequiredResponseMessageWithSubProtocol(HttpRequestMessage request, string subprotocol)
        {
            HttpResponseMessage response = GetUpgradeRequiredResponseMessage(request);
            if (!string.IsNullOrEmpty(subprotocol))
            {
                response.Headers.Add(WebSocketHelper.SecWebSocketProtocol, subprotocol);
            }

            return response;
        }

        static HttpResponseMessage GetUpgradeRequiredResponseMessageWithVersion(HttpRequestMessage request, string version)
        {
            HttpResponseMessage response = GetUpgradeRequiredResponseMessage(request);
            response.Headers.Add(WebSocketHelper.SecWebSocketVersion, version);

            return response;
        }

        static bool CheckHttpHeader(HttpRequestMessage request, string header, Func<string, bool> validator)
        {
            Fx.Assert(request != null, "request should not be null.");
            Fx.Assert(header != null, "header should not be null.");
            Fx.Assert(validator != null, "validator should not be null.");

            IEnumerable<string> headerValues;
            if (!request.Headers.TryGetValues(header, out headerValues))
            {
                return false;
            }

            bool isValid = false;
            foreach (string headerValue in headerValues)
            {
                if (headerValue != null)
                {
                    isValid = validator(headerValue.Trim());
                    if (!isValid)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        bool CheckVersion(string headerValue)
        {
            Fx.Assert(headerValue != null, "headerValue should not be null.");
            return headerValue == this.currentVersion;
        }

        bool CheckContentType(string headerValue)
        {
            Fx.Assert(headerValue != null, "headerValue should not be null.");
            return this.encoder.IsContentTypeSupported(headerValue);
        }

        bool CheckTransferMode(string headerValue)
        {
            Fx.Assert(headerValue != null, "headerValue should not be null.");
            return headerValue.Equals(this.transferMode, StringComparison.OrdinalIgnoreCase);
        }
        
        HttpResponseMessage GetBadRequestResponseMessageWithContentTypeAndTransfermode(HttpRequestMessage request)
        {
            Fx.Assert(this.needToCheckContentType, "needToCheckContentType should be true.");
            HttpResponseMessage response = GetBadRequestResponseMessage(request);
            response.Headers.Add(WebSocketTransportSettings.SoapContentTypeHeader, this.encoder.ContentType);
            if (this.needToCheckTransferMode)
            {
                response.Headers.Add(WebSocketTransportSettings.BinaryEncoderTransferModeHeader, this.transferMode.ToString());
            }

            return response;
        }

        struct SubprotocolParseResult
        {
            public static readonly SubprotocolParseResult HeaderInvalid = new SubprotocolParseResult(true, false, null);
            public static readonly SubprotocolParseResult HeaderNotFound = new SubprotocolParseResult(false, false, null);

            bool headerFound;
            bool headerValid;
            IEnumerable<string> parsedSubprotocols;

            public SubprotocolParseResult(bool headerFound, bool headerValid, IEnumerable<string> parsedSubprotocols)
            {
                this.headerFound = headerFound;
                this.headerValid = headerValid;
                this.parsedSubprotocols = parsedSubprotocols;
            }

            public bool HeaderFound
            {
                get { return this.headerFound; }
            }

            public bool HeaderValid
            {
                get { return this.headerValid; }
            }

            public IEnumerable<string> ParsedSubprotocols
            {
                get { return this.parsedSubprotocols; }
            }
        }
    }
}
