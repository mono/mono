//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ComponentModel;

    public sealed class BasicHttpSecurity
    {
        internal const BasicHttpSecurityMode DefaultMode = BasicHttpSecurityMode.None;
        BasicHttpSecurityMode mode;
        HttpTransportSecurity transportSecurity;
        BasicHttpMessageSecurity messageSecurity;

        public BasicHttpSecurity()
            : this(DefaultMode, new HttpTransportSecurity(), new BasicHttpMessageSecurity())
        {
        }

        BasicHttpSecurity(BasicHttpSecurityMode mode, HttpTransportSecurity transportSecurity, BasicHttpMessageSecurity messageSecurity)
        {
            Fx.Assert(BasicHttpSecurityModeHelper.IsDefined(mode), string.Format("Invalid BasicHttpSecurityMode value: {0}.", mode.ToString()));
            this.Mode = mode;
            this.transportSecurity = transportSecurity == null ? new HttpTransportSecurity() : transportSecurity;
            this.messageSecurity = messageSecurity == null ? new BasicHttpMessageSecurity() : messageSecurity;
        }

        public BasicHttpSecurityMode Mode
        {
            get { return this.mode; }
            set
            {
                if (!BasicHttpSecurityModeHelper.IsDefined(value))
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
                this.transportSecurity = (value == null) ? new HttpTransportSecurity() : value;
            }
        }

        public BasicHttpMessageSecurity Message
        {
            get { return this.messageSecurity; }
            set
            {
                this.messageSecurity = (value == null) ? new BasicHttpMessageSecurity() : value;
            }
        }

        internal void EnableTransportSecurity(HttpsTransportBindingElement https)
        {
            if (this.mode == BasicHttpSecurityMode.TransportWithMessageCredential)
            {
                this.transportSecurity.ConfigureTransportProtectionOnly(https);
            }
            else
            {
                this.transportSecurity.ConfigureTransportProtectionAndAuthentication(https);
            }
        }

        internal static void EnableTransportSecurity(HttpsTransportBindingElement https, HttpTransportSecurity transportSecurity)
        {
            HttpTransportSecurity.ConfigureTransportProtectionAndAuthentication(https, transportSecurity);
        }

        internal void EnableTransportAuthentication(HttpTransportBindingElement http)
        {
            this.transportSecurity.ConfigureTransportAuthentication(http);
        }

        internal static bool IsEnabledTransportAuthentication(HttpTransportBindingElement http, HttpTransportSecurity transportSecurity)
        {
            return HttpTransportSecurity.IsConfiguredTransportAuthentication(http, transportSecurity);
        }

        internal void DisableTransportAuthentication(HttpTransportBindingElement http)
        {
            this.transportSecurity.DisableTransportAuthentication(http);
        }

        internal SecurityBindingElement CreateMessageSecurity()
        {
            if (this.mode == BasicHttpSecurityMode.Message
                || this.mode == BasicHttpSecurityMode.TransportWithMessageCredential)
            {
                return this.messageSecurity.CreateMessageSecurity(this.Mode == BasicHttpSecurityMode.TransportWithMessageCredential);
            }
            else
            {
                return null;
            }
        }

        internal static bool TryCreate(SecurityBindingElement sbe, UnifiedSecurityMode mode, HttpTransportSecurity transportSecurity, out BasicHttpSecurity security)
        {
            security = null;
            BasicHttpMessageSecurity messageSecurity = null;
            if (sbe != null)
            {
                mode &= UnifiedSecurityMode.Message | UnifiedSecurityMode.TransportWithMessageCredential;
                bool isSecureTransportMode;
                if (!BasicHttpMessageSecurity.TryCreate(sbe, out messageSecurity, out isSecureTransportMode))
                {
                    return false;
                }
            }
            else
            {
                mode &= ~(UnifiedSecurityMode.Message | UnifiedSecurityMode.TransportWithMessageCredential);
            }
            BasicHttpSecurityMode basicHttpSecurityMode = BasicHttpSecurityModeHelper.ToSecurityMode(mode);
            Fx.Assert(BasicHttpSecurityModeHelper.IsDefined(basicHttpSecurityMode), string.Format("Invalid BasicHttpSecurityMode value: {0}.", basicHttpSecurityMode.ToString()));
            security = new BasicHttpSecurity(basicHttpSecurityMode, transportSecurity, messageSecurity);

            return SecurityElement.AreBindingsMatching(security.CreateMessageSecurity(), sbe);
        }

        internal bool InternalShouldSerialize()
        {
            return this.Mode != DefaultMode
                || this.ShouldSerializeMessage()
                || this.ShouldSerializeTransport();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeMessage()
        {
            return messageSecurity.InternalShouldSerialize();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeTransport()
        {
            return transportSecurity.InternalShouldSerialize();
        }
    }
}
