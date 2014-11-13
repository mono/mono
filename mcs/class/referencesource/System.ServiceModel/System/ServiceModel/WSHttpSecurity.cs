//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ComponentModel;

    public sealed class WSHttpSecurity
    {
        internal const SecurityMode DefaultMode = SecurityMode.Message;

        SecurityMode mode;
        HttpTransportSecurity transportSecurity;
        NonDualMessageSecurityOverHttp messageSecurity;

        public WSHttpSecurity()
            : this(DefaultMode, GetDefaultHttpTransportSecurity(), new NonDualMessageSecurityOverHttp())
        {
        }

        internal WSHttpSecurity(SecurityMode mode, HttpTransportSecurity transportSecurity, NonDualMessageSecurityOverHttp messageSecurity)
        {
            this.mode = mode;
            this.transportSecurity = transportSecurity == null ? GetDefaultHttpTransportSecurity() : transportSecurity;
            this.messageSecurity = messageSecurity == null ? new NonDualMessageSecurityOverHttp() : messageSecurity;
        }

        internal static HttpTransportSecurity GetDefaultHttpTransportSecurity()
        {
            HttpTransportSecurity transportSecurity = new HttpTransportSecurity();
            transportSecurity.ClientCredentialType = HttpClientCredentialType.Windows;
            return transportSecurity;
        }

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

        public HttpTransportSecurity Transport
        {
            get { return this.transportSecurity; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                }
                this.transportSecurity = value;
            }
        }

        public NonDualMessageSecurityOverHttp Message
        {
            get { return this.messageSecurity; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                }
                this.messageSecurity = value;
            }
        }

        internal void ApplyTransportSecurity(HttpsTransportBindingElement https)
        {
            if (this.mode == SecurityMode.TransportWithMessageCredential)
            {
                this.transportSecurity.ConfigureTransportProtectionOnly(https);
            }
            else
            {
                this.transportSecurity.ConfigureTransportProtectionAndAuthentication(https);
            }
        }

        internal static void ApplyTransportSecurity(HttpsTransportBindingElement transport, HttpTransportSecurity transportSecurity)
        {
            HttpTransportSecurity.ConfigureTransportProtectionAndAuthentication(transport, transportSecurity);
        }

        internal SecurityBindingElement CreateMessageSecurity(bool isReliableSessionEnabled, MessageSecurityVersion version)
        {
            if (this.mode == SecurityMode.Message || this.mode == SecurityMode.TransportWithMessageCredential)
            {
                return this.messageSecurity.CreateSecurityBindingElement(this.Mode == SecurityMode.TransportWithMessageCredential, isReliableSessionEnabled, version);
            }
            else
            {
                return null;
            }
        }

        internal static bool TryCreate(SecurityBindingElement sbe, UnifiedSecurityMode mode, HttpTransportSecurity transportSecurity, bool isReliableSessionEnabled, out WSHttpSecurity security)
        {
            security = null;
            NonDualMessageSecurityOverHttp messageSecurity = null;
            SecurityMode securityMode = SecurityMode.None;
            if (sbe != null)
            {
                mode &= UnifiedSecurityMode.Message | UnifiedSecurityMode.TransportWithMessageCredential;
                securityMode = SecurityModeHelper.ToSecurityMode(mode);
                Fx.Assert(SecurityModeHelper.IsDefined(securityMode), string.Format("Invalid SecurityMode value: {0}.", mode.ToString()));
                if (!MessageSecurityOverHttp.TryCreate(sbe, securityMode == SecurityMode.TransportWithMessageCredential, isReliableSessionEnabled, out messageSecurity))
                {
                    return false;
                }
            }
            else
            {
                mode &= ~(UnifiedSecurityMode.Message | UnifiedSecurityMode.TransportWithMessageCredential);
                securityMode = SecurityModeHelper.ToSecurityMode(mode);
            }
            Fx.Assert(SecurityModeHelper.IsDefined(securityMode), string.Format("Invalid SecurityMode value: {0}.", securityMode.ToString()));
            security = new WSHttpSecurity(securityMode, transportSecurity, messageSecurity);
            return true;
        }

        internal bool InternalShouldSerialize()
        {
            return this.ShouldSerializeMode()
                || this.ShouldSerializeMessage()
                || this.ShouldSerializeTransport();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeMode()
        {
            return this.Mode != DefaultMode;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeMessage()
        {
            return this.Message.InternalShouldSerialize();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeTransport()
        {
            return this.Transport.ClientCredentialType != HttpClientCredentialType.Windows
                || this.Transport.ShouldSerializeProxyCredentialType()
                || this.Transport.ShouldSerializeRealm();
        }

    }
}
