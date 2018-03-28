//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ComponentModel;

    public sealed class WebHttpSecurity
    {
        internal const WebHttpSecurityMode DefaultMode = WebHttpSecurityMode.None;
        WebHttpSecurityMode mode;
        HttpTransportSecurity transportSecurity;
        bool isModeSet;

        public WebHttpSecurity()
        {
            this.transportSecurity = new HttpTransportSecurity();
        }

        public WebHttpSecurityMode Mode
        {
            get { return this.mode; }
            set
            {
                if (!WebHttpSecurityModeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.mode = value;
                this.isModeSet = true;
            }
        }

        internal bool IsModeSet
        {
            get { return this.isModeSet; }
        }

        public HttpTransportSecurity Transport
        {
            get { return this.transportSecurity; }
            set
            {
                this.transportSecurity = (value == null) ? new HttpTransportSecurity() : value;
            }
        }

        internal void DisableTransportAuthentication(HttpTransportBindingElement http)
        {
            this.transportSecurity.DisableTransportAuthentication(http);
        }

        internal void EnableTransportAuthentication(HttpTransportBindingElement http)
        {
            this.transportSecurity.ConfigureTransportAuthentication(http);
        }

        internal void EnableTransportSecurity(HttpsTransportBindingElement https)
        {
            this.transportSecurity.ConfigureTransportProtectionAndAuthentication(https);
        }

        internal bool InternalShouldSerialize()
        {
            return this.ShouldSerializeMode()
                || this.ShouldSerializeTransport();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeMode()
        {
            return this.Mode != DefaultMode;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeTransport()
        {
            return this.Transport.InternalShouldSerialize();
        }


    }
}
