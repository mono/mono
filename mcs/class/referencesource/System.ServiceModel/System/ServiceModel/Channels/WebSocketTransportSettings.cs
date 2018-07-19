// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Channels
{
    using System;
    using System.ComponentModel;
    using System.Net.WebSockets;
    using System.Runtime;
    using System.Threading;

    public sealed class WebSocketTransportSettings : IEquatable<WebSocketTransportSettings>
    {
        public const string ConnectionOpenedAction = "http://schemas.microsoft.com/2011/02/session/onopen";
        public const string BinaryMessageReceivedAction = "http://schemas.microsoft.com/2011/02/websockets/onbinarymessage";
        public const string TextMessageReceivedAction = "http://schemas.microsoft.com/2011/02/websockets/ontextmessage";
        public const string SoapContentTypeHeader = "soap-content-type";
        public const string BinaryEncoderTransferModeHeader = "microsoft-binary-transfer-mode";
        internal const string WebSocketMethod = "WEBSOCKET";
        internal const string SoapSubProtocol = "soap";
        internal const string TransportUsageMethodName = "TransportUsage";

        WebSocketTransportUsage transportUsage;
        bool createNotificationOnConnection;
        TimeSpan keepAliveInterval;
        string subProtocol;
        bool disablePayloadMasking;
        int maxPendingConnections;

        public WebSocketTransportSettings()
        {
            this.transportUsage = WebSocketDefaults.TransportUsage;
            this.createNotificationOnConnection = WebSocketDefaults.CreateNotificationOnConnection;
            this.keepAliveInterval = WebSocketDefaults.DefaultKeepAliveInterval;
            this.subProtocol = WebSocketDefaults.SubProtocol;
            this.disablePayloadMasking = WebSocketDefaults.DisablePayloadMasking;
            this.maxPendingConnections = WebSocketDefaults.DefaultMaxPendingConnections;
        }

        WebSocketTransportSettings(WebSocketTransportSettings settings)
        {
            Fx.Assert(settings != null, "settings should not be null.");
            this.TransportUsage = settings.TransportUsage;
            this.SubProtocol = settings.SubProtocol;
            this.KeepAliveInterval = settings.KeepAliveInterval;
            this.DisablePayloadMasking = settings.DisablePayloadMasking;
            this.CreateNotificationOnConnection = settings.CreateNotificationOnConnection;
            this.MaxPendingConnections = settings.MaxPendingConnections;
        }

        [DefaultValue(WebSocketDefaults.TransportUsage)]
        public WebSocketTransportUsage TransportUsage
        {
            get
            {
                return this.transportUsage;
            }

            set
            {
                WebSocketTransportUsageHelper.Validate(value);
                this.transportUsage = value;
            }
        }

        [DefaultValue(WebSocketDefaults.CreateNotificationOnConnection)]
        public bool CreateNotificationOnConnection
        {
            get
            {
                return this.createNotificationOnConnection;
            }

            set
            {
                this.createNotificationOnConnection = value;
            }
        }

        [DefaultValue(typeof(TimeSpan), WebSocketDefaults.DefaultKeepAliveIntervalString)]
        public TimeSpan KeepAliveInterval
        {
            get
            {
                return this.keepAliveInterval;
            }

            set
            {
                if (value < TimeSpan.Zero && value != Timeout.InfiniteTimeSpan)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException(
                                "value", 
                                value,
                                SR.GetString(SR.SFxTimeoutOutOfRange0)));
                }

                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException(
                                            "value", 
                                            value,
                                            SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
                }

                this.keepAliveInterval = value;
            }
        }
        
        [DefaultValue(WebSocketDefaults.SubProtocol)]
        public string SubProtocol
        {
            get
            {
                return this.subProtocol;
            }

            set
            {
                if (value != null)
                {
                    if (value == string.Empty)
                    {
                        throw FxTrace.Exception.Argument("value", SR.GetString(SR.WebSocketInvalidProtocolEmptySubprotocolString));
                    }

                    if (value.Split(WebSocketHelper.ProtocolSeparators).Length > 1)
                    {
                        throw FxTrace.Exception.Argument("value", SR.GetString(SR.WebSocketInvalidProtocolContainsMultipleSubProtocolString, value));
                    }

                    string invalidChar;
                    if (WebSocketHelper.IsSubProtocolInvalid(value, out invalidChar))
                    {
                        throw FxTrace.Exception.Argument("value", SR.GetString(SR.WebSocketInvalidProtocolInvalidCharInProtocolString, value, invalidChar));
                    }
                }

                this.subProtocol = value;
            }
        }

        [DefaultValue(WebSocketDefaults.DisablePayloadMasking)]
        public bool DisablePayloadMasking
        {
            get
            {
                return this.disablePayloadMasking;
            }

            set
            {
                this.disablePayloadMasking = value;
            }
        }

        [DefaultValue(WebSocketDefaults.DefaultMaxPendingConnections)]
        public int MaxPendingConnections
        {
            get
            {
                return this.maxPendingConnections;
            }

            set
            {
                if (value < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException(
                        "value",
                        value,
                        SR.GetString(SR.ValueMustBePositive)));
                }

                this.maxPendingConnections = value;
            }
        }

        public bool Equals(WebSocketTransportSettings other)
        {
            if (other == null)
            {
                return false;
            }

            return this.TransportUsage == other.TransportUsage
                && this.CreateNotificationOnConnection == other.CreateNotificationOnConnection
                && this.KeepAliveInterval == other.KeepAliveInterval
                && this.DisablePayloadMasking == other.DisablePayloadMasking
                && StringComparer.OrdinalIgnoreCase.Compare(this.SubProtocol, other.SubProtocol) == 0
                && this.MaxPendingConnections == other.MaxPendingConnections;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return base.Equals(obj);
            }

            WebSocketTransportSettings settings = obj as WebSocketTransportSettings;
            return this.Equals(settings);
        }

        public override int GetHashCode()
        {
            int hashcode = this.TransportUsage.GetHashCode() 
                        ^ this.CreateNotificationOnConnection.GetHashCode()
                        ^ this.KeepAliveInterval.GetHashCode()
                        ^ this.DisablePayloadMasking.GetHashCode()
                        ^ this.MaxPendingConnections.GetHashCode();
            if (this.SubProtocol != null)
            {
                hashcode ^= this.SubProtocol.ToLowerInvariant().GetHashCode();
            }

            return hashcode;
        }

        internal WebSocketTransportSettings Clone()
        {
            return new WebSocketTransportSettings(this);
        }

        internal TimeSpan GetEffectiveKeepAliveInterval()
        {
            return this.keepAliveInterval == TimeSpan.Zero ? WebSocket.DefaultKeepAliveInterval : this.keepAliveInterval;
        }
    }
}
