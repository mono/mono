// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.WebSockets;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    static class WebSocketHelper
    {
        internal const int OperationNotStarted = 0;
        internal const int OperationFinished = 1;

        internal const string SecWebSocketKey = "Sec-WebSocket-Key";
        internal const string SecWebSocketVersion = "Sec-WebSocket-Version";
        internal const string SecWebSocketProtocol = "Sec-WebSocket-Protocol";
        internal const string SecWebSocketAccept = "Sec-WebSocket-Accept";
        internal const string MaxPendingConnectionsString = "MaxPendingConnections";
        internal const string WebSocketTransportSettingsString = "WebSocketTransportSettings";

        internal const string CloseOperation = "CloseOperation";
        internal const string SendOperation = "SendOperation";
        internal const string ReceiveOperation = "ReceiveOperation";

        internal static readonly char[] ProtocolSeparators = new char[] { ',' };

        const string WebSocketKeyPostString = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        const string SchemeWs = "ws";
        const string SchemeWss = "wss";

        static readonly int PropertyBufferSize = ((2 * Marshal.SizeOf(typeof(uint))) + Marshal.SizeOf(typeof(bool))) + IntPtr.Size;
        static readonly HashSet<char> InvalidSeparatorSet = new HashSet<char>(new char[] { '(', ')', '<', '>', '@', ',', ';', ':', '\\', '"', '/', '[', ']', '?', '=', '{', '}', ' ' });
        static string currentWebSocketVersion;

        [SuppressMessage("Microsoft.Security.Cryptography", "CA5354:DoNotUseSHA1", Justification = "Cannot change. Usage of SHA1 is part of WebSocket spec. Justification in RFC6455 section 10.8")]
        internal static string ComputeAcceptHeader(string webSocketKey)
        {
            Fx.Assert(webSocketKey != null, "webSocketKey should not be null.");
            using (SHA1 sha = SHA1.Create())
            {
                string fullString = webSocketKey + WebSocketHelper.WebSocketKeyPostString;
                byte[] bytes = Encoding.UTF8.GetBytes(fullString);
                return Convert.ToBase64String(sha.ComputeHash(bytes));
            }
        }

        internal static int ComputeClientBufferSize(long maxReceivedMessageSize)
        {
            return ComputeInternalBufferSize(maxReceivedMessageSize, false);
        }

        internal static int ComputeServerBufferSize(long maxReceivedMessageSize)
        {
            return ComputeInternalBufferSize(maxReceivedMessageSize, true);
        }

        internal static int GetReceiveBufferSize(long maxReceivedMessageSize)
        {
            int effectiveMaxReceiveBufferSize = maxReceivedMessageSize <= WebSocketDefaults.BufferSize ? (int)maxReceivedMessageSize : WebSocketDefaults.BufferSize;
            return Math.Max(WebSocketDefaults.MinReceiveBufferSize, effectiveMaxReceiveBufferSize);
        }

        internal static bool UseWebSocketTransport(WebSocketTransportUsage transportUsage, bool isContractDuplex)
        {
            return transportUsage == WebSocketTransportUsage.Always
                || (transportUsage == WebSocketTransportUsage.WhenDuplex && isContractDuplex);
        }

        internal static Uri GetWebSocketUri(Uri httpUri)
        {
            Fx.Assert(httpUri != null, "RemoteAddress.Uri should not be null.");
            UriBuilder builder = new UriBuilder(httpUri);

            if (Uri.UriSchemeHttp.Equals(httpUri.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                builder.Scheme = SchemeWs;
            }
            else
            {
                Fx.Assert(
                    Uri.UriSchemeHttps.Equals(httpUri.Scheme, StringComparison.OrdinalIgnoreCase),
                    "httpUri.Scheme should be http or https.");
                builder.Scheme = SchemeWss;
            }

            return builder.Uri;
        }

        internal static bool IsWebSocketUri(Uri uri)
        {
            return uri != null && 
                (WebSocketHelper.SchemeWs.Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase) ||
                 WebSocketHelper.SchemeWss.Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase));
        }

        internal static Uri NormalizeWsSchemeWithHttpScheme(Uri uri)
        {
            Fx.Assert(uri != null, "RemoteAddress.Uri should not be null.");
            if (!IsWebSocketUri(uri))
            {
                return uri;
            }

            UriBuilder builder = new UriBuilder(uri);

            switch (uri.Scheme.ToLowerInvariant())
            {
                case SchemeWs:
                    builder.Scheme = Uri.UriSchemeHttp;
                    break;
                case SchemeWss:
                    builder.Scheme = Uri.UriSchemeHttps;
                    break;
                default:
                    break;
            }

            return builder.Uri;
        }

        internal static bool TryParseSubProtocol(string subProtocolValue, out List<string> subProtocolList)
        {
            subProtocolList = new List<string>();
            if (subProtocolValue != null)
            {
                string[] parsedTokens = subProtocolValue.Split(ProtocolSeparators, StringSplitOptions.RemoveEmptyEntries);

                string invalidChar;
                for (int i = 0; i < parsedTokens.Length; i++)
                {
                    string token = parsedTokens[i];
                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        token = token.Trim();
                        if (!IsSubProtocolInvalid(token, out invalidChar))
                        {
                            // Note that we could be adding a duplicate to this list. According to the specification the header should not include
                            // duplicates but we aim to be "robust in what we receive" so we will allow it. The matching code that consumes this list
                            // will take the first match so duplicates will not affect the outcome of the negotiation process.
                            subProtocolList.Add(token);
                        }
                        else
                        {
                            FxTrace.Exception.AsWarning(new WebException(
                                SR.GetString(SR.WebSocketInvalidProtocolInvalidCharInProtocolString, token, invalidChar)));
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        internal static bool IsSubProtocolInvalid(string protocol, out string invalidChar)
        {
            Fx.Assert(protocol != null, "protocol should not be null");
            char[] chars = protocol.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                char ch = chars[i];
                if (ch < 0x21 || ch > 0x7e)
                {
                    invalidChar = string.Format(CultureInfo.InvariantCulture, "[{0}]", (int)ch);
                    return true;
                }

                if (InvalidSeparatorSet.Contains(ch))
                {
                    invalidChar = ch.ToString();
                    return true;
                }
            }

            invalidChar = null;
            return false;
        }

        internal static string GetCurrentVersion()
        {
            if (currentWebSocketVersion == null)
            {
                WebSocket.RegisterPrefixes();
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("ws://localhost");
                string version = request.Headers[WebSocketHelper.SecWebSocketVersion];
                Fx.Assert(version != null, "version should not be null.");
                currentWebSocketVersion = version.Trim();
            }

            return currentWebSocketVersion;
        }

        internal static WebSocketTransportSettings GetRuntimeWebSocketSettings(WebSocketTransportSettings settings)
        {
            WebSocketTransportSettings runtimeSettings = settings.Clone();
            if (runtimeSettings.MaxPendingConnections == WebSocketDefaults.DefaultMaxPendingConnections)
            {
                runtimeSettings.MaxPendingConnections = WebSocketDefaults.MaxPendingConnectionsCpuCount;
            }

            return runtimeSettings;
        }
        
        internal static bool OSSupportsWebSockets()
        {
            return OSEnvironmentHelper.IsAtLeast(OSVersion.Win8);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(FxCop.Category.ReliabilityBasic, FxCop.Rule.WrapExceptionsRule,
                    Justification = "The exceptions thrown here are already wrapped.")]
        internal static void ThrowCorrectException(Exception ex)
        {
            throw ConvertAndTraceException(ex);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(FxCop.Category.ReliabilityBasic, FxCop.Rule.WrapExceptionsRule,
                    Justification = "The exceptions thrown here are already wrapped.")]
        internal static void ThrowCorrectException(Exception ex, TimeSpan timeout, string operation)
        {
            throw ConvertAndTraceException(ex, timeout, operation);
        }

        internal static Exception ConvertAndTraceException(Exception ex)
        {
            return ConvertAndTraceException(
                    ex,
                    TimeSpan.MinValue, // this is a dummy since operation type is null, so the timespan value won't be used
                    null);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(FxCop.Category.ReliabilityBasic, "Reliability103:ThrowWrappedExceptionsRule",
                    Justification = "The exceptions wrapped here will be thrown out later.")]
        internal static Exception ConvertAndTraceException(Exception ex, TimeSpan timeout, string operation)
        {
            ObjectDisposedException objectDisposedException = ex as ObjectDisposedException;
            if (objectDisposedException != null)
            {
                CommunicationObjectAbortedException communicationObjectAbortedException = new CommunicationObjectAbortedException(ex.Message, ex);
                FxTrace.Exception.AsWarning(communicationObjectAbortedException);
                return communicationObjectAbortedException;
            }

            AggregateException aggregationException = ex as AggregateException;
            if (aggregationException != null)
            {
                Exception exception = FxTrace.Exception.AsError<OperationCanceledException>(aggregationException);
                OperationCanceledException operationCanceledException = exception as OperationCanceledException;
                if (operationCanceledException != null)
                {
                    TimeoutException timeoutException = GetTimeoutException(exception, timeout, operation);
                    FxTrace.Exception.AsWarning(timeoutException);
                    return timeoutException;
                }
                else
                {
                    Exception communicationException = ConvertAggregateExceptionToCommunicationException(aggregationException);
                    if (communicationException is CommunicationObjectAbortedException)
                    {
                        FxTrace.Exception.AsWarning(communicationException);
                        return communicationException;
                    }
                    else
                    {
                        return FxTrace.Exception.AsError(communicationException);
                    }
                }
            }

            WebSocketException webSocketException = ex as WebSocketException;
            if (webSocketException != null)
            {
                switch (webSocketException.WebSocketErrorCode)
                {
                    case WebSocketError.InvalidMessageType:
                    case WebSocketError.UnsupportedProtocol:
                    case WebSocketError.UnsupportedVersion:
                        ex = new ProtocolException(ex.Message, ex);
                        break;
                    default:
                        ex = new CommunicationException(ex.Message, ex);
                        break;
                }
            }

            return FxTrace.Exception.AsError(ex);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(FxCop.Category.ReliabilityBasic, "Reliability103",
                            Justification = "The exceptions will be wrapped by the callers.")]
        internal static Exception ConvertAggregateExceptionToCommunicationException(AggregateException ex)
        {
            Exception exception = FxTrace.Exception.AsError<WebSocketException>(ex);
            WebSocketException webSocketException = exception as WebSocketException;
            if (webSocketException != null && webSocketException.InnerException != null)
            {
                HttpListenerException httpListenerException = webSocketException.InnerException as HttpListenerException;
                if (httpListenerException != null)
                {
                    return HttpChannelUtilities.CreateCommunicationException(httpListenerException);
                }
            }

            ObjectDisposedException objectDisposedException = exception as ObjectDisposedException;
            if (objectDisposedException != null)
            {
                return new CommunicationObjectAbortedException(exception.Message, exception);
            }

            return new CommunicationException(exception.Message, exception);
        }

        internal static void ThrowExceptionOnTaskFailure(Task task, TimeSpan timeout, string operation)
        {
            if (task.IsFaulted)
            {
                throw FxTrace.Exception.AsError<CommunicationException>(task.Exception);
            }
            else if (task.IsCanceled)
            {
                throw FxTrace.Exception.AsError(GetTimeoutException(null, timeout, operation));
            }
        }

        internal static TimeoutException GetTimeoutException(Exception innerException, TimeSpan timeout, string operation)
        {
            string errorMsg = string.Empty;
            if (operation != null)
            {
                switch (operation)
                {
                    case WebSocketHelper.CloseOperation:
                        errorMsg = SR.GetString(SR.CloseTimedOut, timeout);
                        break;
                    case WebSocketHelper.SendOperation:
                        errorMsg = SR.GetString(SR.WebSocketSendTimedOut, timeout);
                        break;
                    case WebSocketHelper.ReceiveOperation:
                        errorMsg = SR.GetString(SR.WebSocketReceiveTimedOut, timeout);
                        break;
                    default:
                        errorMsg = SR.GetString(SR.WebSocketOperationTimedOut, operation, timeout);
                        break;
                }
            }

            return innerException == null ? new TimeoutException(errorMsg) : new TimeoutException(errorMsg, innerException);
        }

        private static int ComputeInternalBufferSize(long maxReceivedMessageSize, bool isServerBuffer)
        {
            const int NativeOverheadBufferSize = 144;
            /* LAYOUT:
            | Native buffer              | PayloadReceiveBuffer | PropertyBuffer |
            | RBS + SBS + 144            | RBS                  | PBS            |
            | Only WSPC may modify       | Only WebSocketBase may modify         | 

             *RBS = ReceiveBufferSize, *SBS = SendBufferSize
             *PBS = PropertyBufferSize (32-bit: 16, 64 bit: 20 bytes) */

            int nativeSendBufferSize = isServerBuffer ? WebSocketDefaults.MinSendBufferSize : WebSocketDefaults.BufferSize;
            return (2 * GetReceiveBufferSize(maxReceivedMessageSize)) + nativeSendBufferSize + NativeOverheadBufferSize + PropertyBufferSize;
        }
    }
}
