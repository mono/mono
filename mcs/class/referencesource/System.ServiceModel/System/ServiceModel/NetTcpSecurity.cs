//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System.ComponentModel;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;

    public sealed class NetTcpSecurity
    {
        internal const SecurityMode DefaultMode = SecurityMode.Transport;

        SecurityMode mode;
        TcpTransportSecurity transportSecurity;
        MessageSecurityOverTcp messageSecurity;

        public NetTcpSecurity()
            : this(DefaultMode, new TcpTransportSecurity(), new MessageSecurityOverTcp())
        {
        }

        NetTcpSecurity(SecurityMode mode, TcpTransportSecurity transportSecurity, MessageSecurityOverTcp messageSecurity)
        {
            Fx.Assert(SecurityModeHelper.IsDefined(mode), string.Format("Invalid SecurityMode value: {0}.", mode.ToString()));

            this.mode = mode;
            this.transportSecurity = transportSecurity == null ? new TcpTransportSecurity() : transportSecurity;
            this.messageSecurity = messageSecurity == null ? new MessageSecurityOverTcp() : messageSecurity;
        }

        [DefaultValue(DefaultMode)]
        public SecurityMode Mode
        {
            get { return this.mode; }
            set
            {
                if (!SecurityModeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.mode = value;
            }
        }

        public TcpTransportSecurity Transport
        {
            get { return this.transportSecurity; }
            set { this.transportSecurity = value; }
        }

        public MessageSecurityOverTcp Message
        {
            get { return this.messageSecurity; }
            set { this.messageSecurity = value; }
        }


        internal BindingElement CreateTransportSecurity()
        {
            if (this.mode == SecurityMode.TransportWithMessageCredential)
            {
                return this.transportSecurity.CreateTransportProtectionOnly();
            }
            else if (this.mode == SecurityMode.Transport)
            {
                return this.transportSecurity.CreateTransportProtectionAndAuthentication();
            }
            else
            {
                return null;
            }
        }

        internal static UnifiedSecurityMode GetModeFromTransportSecurity(BindingElement transport)
        {
            if (transport == null)
            {
                return UnifiedSecurityMode.None | UnifiedSecurityMode.Message;
            }
            else
            {
                return UnifiedSecurityMode.TransportWithMessageCredential | UnifiedSecurityMode.Transport;
            }
        }

        internal static bool SetTransportSecurity(BindingElement transport, SecurityMode mode, TcpTransportSecurity transportSecurity)
        {
            if (mode == SecurityMode.TransportWithMessageCredential)
            {
                return TcpTransportSecurity.SetTransportProtectionOnly(transport, transportSecurity);
            }
            else if (mode == SecurityMode.Transport)
            {
                return TcpTransportSecurity.SetTransportProtectionAndAuthentication(transport, transportSecurity);
            }
            return transport == null;
        }

        internal SecurityBindingElement CreateMessageSecurity(bool isReliableSessionEnabled)
        {
            if (this.mode == SecurityMode.Message)
            {
                return this.messageSecurity.CreateSecurityBindingElement(false, isReliableSessionEnabled, null);
            }
            else if (this.mode == SecurityMode.TransportWithMessageCredential)
            {
                return this.messageSecurity.CreateSecurityBindingElement(true, isReliableSessionEnabled, this.CreateTransportSecurity());
            }
            else
            {
                return null;
            }
        }

        internal static bool TryCreate(SecurityBindingElement wsSecurity, SecurityMode mode, bool isReliableSessionEnabled, BindingElement transportSecurity, TcpTransportSecurity tcpTransportSecurity, out NetTcpSecurity security)
        {
            security = null;
            MessageSecurityOverTcp messageSecurity = null;
            if (mode == SecurityMode.Message)
            {
                if (!MessageSecurityOverTcp.TryCreate(wsSecurity, isReliableSessionEnabled, null, out messageSecurity))
                    return false;
            }
            else if (mode == SecurityMode.TransportWithMessageCredential)
            {
                if (!MessageSecurityOverTcp.TryCreate(wsSecurity, isReliableSessionEnabled, transportSecurity, out messageSecurity))
                    return false;
            }
            security = new NetTcpSecurity(mode, tcpTransportSecurity, messageSecurity);
            return SecurityElement.AreBindingsMatching(security.CreateMessageSecurity(isReliableSessionEnabled), wsSecurity, false);
        }

        internal bool InternalShouldSerialize()
        {
            return this.Mode != NetTcpSecurity.DefaultMode
                || this.Transport.InternalShouldSerialize()
                || this.Message.InternalShouldSerialize();
        }
    }
}

