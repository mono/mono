//------------------------------------------------------------------------------
// <copyright file="WebSocketException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net.WebSockets
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions; 

    [Serializable]
    public sealed class WebSocketException : Win32Exception
    {
        private WebSocketError m_WebSocketErrorCode;

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", 
            Justification = "This ctor is harmless, because it does not pass arbitrary data into the native code.")]
        public WebSocketException()
            : this(Marshal.GetLastWin32Error())
        {         
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands",
            Justification = "This ctor is harmless, because it does not pass arbitrary data into the native code.")]
        public WebSocketException(WebSocketError error)
            : this(error, GetErrorMessage(error))
        {
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands",
            Justification = "This ctor is harmless, because it does not pass arbitrary data into the native code.")]
        public WebSocketException(WebSocketError error, string message) : base(message)
        {
            m_WebSocketErrorCode = error;
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands",
            Justification = "This ctor is harmless, because it does not pass arbitrary data into the native code.")]
        public WebSocketException(WebSocketError error, Exception innerException)
            : this(error, GetErrorMessage(error), innerException)
        {
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands",
            Justification = "This ctor is harmless, because it does not pass arbitrary data into the native code.")]
        public WebSocketException(WebSocketError error, string message, Exception innerException) 
            : base(message, innerException)
        {
            m_WebSocketErrorCode = error;
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands",
            Justification = "This ctor is harmless, because it does not pass arbitrary data into the native code.")]
        public WebSocketException(int nativeError)
            : base(nativeError)
        {
            m_WebSocketErrorCode = !WebSocketProtocolComponent.Succeeded(nativeError) ? WebSocketError.NativeError : WebSocketError.Success;
            this.SetErrorCodeOnError(nativeError);
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands",
            Justification = "This ctor is harmless, because it does not pass arbitrary data into the native code.")]
        public WebSocketException(int nativeError, string message) 
            : base(nativeError, message)
        {
            m_WebSocketErrorCode = !WebSocketProtocolComponent.Succeeded(nativeError) ? WebSocketError.NativeError : WebSocketError.Success;
            this.SetErrorCodeOnError(nativeError);
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands",
            Justification = "This ctor is harmless, because it does not pass arbitrary data into the native code.")]
        public WebSocketException(int nativeError, Exception innerException)
            : base(SR.GetString(SR.net_WebSockets_Generic), innerException)
        {
            m_WebSocketErrorCode = !WebSocketProtocolComponent.Succeeded(nativeError) ? WebSocketError.NativeError : WebSocketError.Success;
            this.SetErrorCodeOnError(nativeError);
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands",
            Justification = "This ctor is harmless, because it does not pass arbitrary data into the native code.")]
        public WebSocketException(WebSocketError error, int nativeError)
            : this(error, nativeError, GetErrorMessage(error))
        {
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands",
            Justification = "This ctor is harmless, because it does not pass arbitrary data into the native code.")]
        public WebSocketException(WebSocketError error, int nativeError, string message)
            : base(message)
        {
            m_WebSocketErrorCode = error;
            this.SetErrorCodeOnError(nativeError);
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands",
            Justification = "This ctor is harmless, because it does not pass arbitrary data into the native code.")]
        public WebSocketException(WebSocketError error, int nativeError, Exception innerException)
            : this(error, nativeError, GetErrorMessage(error), innerException)
        {
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands",
            Justification = "This ctor is harmless, because it does not pass arbitrary data into the native code.")]
        public WebSocketException(WebSocketError error, int nativeError, string message, Exception innerException)
            : base(message, innerException)
        {
            m_WebSocketErrorCode = error;
            this.SetErrorCodeOnError(nativeError);
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands",
            Justification = "This ctor is harmless, because it does not pass arbitrary data into the native code.")]
        public WebSocketException(string message)
            : base(message)
        {
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands",
            Justification = "This ctor is harmless, because it does not pass arbitrary data into the native code.")]
        public WebSocketException(string message, Exception innerException)
            : base(message, innerException)
        { 
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands",
            Justification = "This ctor is harmless, because it does not pass arbitrary data into the native code.")]
        private WebSocketException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        public override int ErrorCode
        {
            get
            {
                return base.NativeErrorCode;
            }
        }

        public WebSocketError WebSocketErrorCode
        {
            get
            {
                return m_WebSocketErrorCode; 
            }
        }

        private static string GetErrorMessage(WebSocketError error)
        {
            // provide a canned message for the error type
            switch (error)
            {
                case WebSocketError.InvalidMessageType:
                    return SR.GetString(SR.net_WebSockets_InvalidMessageType_Generic,
                        typeof(WebSocket).Name + WebSocketBase.Methods.CloseAsync,
                        typeof(WebSocket).Name + WebSocketBase.Methods.CloseOutputAsync);
                case WebSocketError.Faulted:
                    return SR.GetString(SR.net_Websockets_WebSocketBaseFaulted);
                case WebSocketError.NotAWebSocket:
                    return SR.GetString(SR.net_WebSockets_NotAWebSocket_Generic);
                case WebSocketError.UnsupportedVersion:
                    return SR.GetString(SR.net_WebSockets_UnsupportedWebSocketVersion_Generic);
                case WebSocketError.UnsupportedProtocol:
                    return SR.GetString(SR.net_WebSockets_UnsupportedProtocol_Generic);
                case WebSocketError.HeaderError:
                    return SR.GetString(SR.net_WebSockets_HeaderError_Generic);
                case WebSocketError.ConnectionClosedPrematurely:
                    return SR.GetString(SR.net_WebSockets_ConnectionClosedPrematurely_Generic);
                case WebSocketError.InvalidState:
                    return SR.GetString(SR.net_WebSockets_InvalidState_Generic);
                default:
                    return SR.GetString(SR.net_WebSockets_Generic);
            }
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("WebSocketErrorCode", m_WebSocketErrorCode);
            base.GetObjectData(info, context);
        }

        // Set the error code only if there is an error (i.e. nativeError >= 0). Otherwise the code ----s up on deserialization 
        // as the Exception..ctor() throws on setting HResult to 0. The default for HResult is -2147467259.
        private void SetErrorCodeOnError(int nativeError)
        {
            if (!WebSocketProtocolComponent.Succeeded(nativeError))
            {
                this.HResult = nativeError;
            }
        }
    }
}
