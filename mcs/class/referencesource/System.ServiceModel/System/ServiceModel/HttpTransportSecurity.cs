//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.Net;
    using System.Net.Security;

    public sealed class HttpTransportSecurity
    {
        internal const HttpClientCredentialType DefaultClientCredentialType = HttpClientCredentialType.None;
        internal const HttpProxyCredentialType DefaultProxyCredentialType = HttpProxyCredentialType.None;
        internal const string DefaultRealm = System.ServiceModel.Channels.HttpTransportDefaults.Realm;

        HttpClientCredentialType clientCredentialType;
        HttpProxyCredentialType proxyCredentialType;
        string realm;
        ExtendedProtectionPolicy extendedProtectionPolicy;

        public HttpTransportSecurity()
        {
            this.clientCredentialType = DefaultClientCredentialType;
            this.proxyCredentialType = DefaultProxyCredentialType;
            this.realm = DefaultRealm;
            this.extendedProtectionPolicy = ChannelBindingUtility.DefaultPolicy;
        }

        public HttpClientCredentialType ClientCredentialType
        {
            get { return this.clientCredentialType; }
            set
            {
                if (!HttpClientCredentialTypeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.clientCredentialType = value;
            }
        }

        public HttpProxyCredentialType ProxyCredentialType
        {
            get { return this.proxyCredentialType; }
            set
            {
                if (!HttpProxyCredentialTypeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.proxyCredentialType = value;
            }
        }

        public string Realm
        {
            get { return this.realm; }
            set { this.realm = value; }
        }

        public ExtendedProtectionPolicy ExtendedProtectionPolicy
        {
            get
            {
                return this.extendedProtectionPolicy;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                if (value.PolicyEnforcement == PolicyEnforcement.Always &&
                    !System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy.OSSupportsExtendedProtection)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new PlatformNotSupportedException(SR.GetString(SR.ExtendedProtectionNotSupported)));
                }

                this.extendedProtectionPolicy = value;
            }
        }

        internal void ConfigureTransportProtectionOnly(HttpsTransportBindingElement https)
        {
            DisableAuthentication(https);
            https.RequireClientCertificate = false;
        }

        void ConfigureAuthentication(HttpTransportBindingElement http)
        {
            http.AuthenticationScheme = HttpClientCredentialTypeHelper.MapToAuthenticationScheme(this.clientCredentialType);
            http.ProxyAuthenticationScheme = HttpProxyCredentialTypeHelper.MapToAuthenticationScheme(this.proxyCredentialType);
            http.Realm = this.Realm;
            http.ExtendedProtectionPolicy = this.extendedProtectionPolicy;
        }

        static void ConfigureAuthentication(HttpTransportBindingElement http, HttpTransportSecurity transportSecurity)
        {
            transportSecurity.clientCredentialType = HttpClientCredentialTypeHelper.MapToClientCredentialType(http.AuthenticationScheme);
            transportSecurity.proxyCredentialType = HttpProxyCredentialTypeHelper.MapToProxyCredentialType(http.ProxyAuthenticationScheme);
            transportSecurity.Realm = http.Realm;
            transportSecurity.extendedProtectionPolicy = http.ExtendedProtectionPolicy;
        }

        void DisableAuthentication(HttpTransportBindingElement http)
        {
            http.AuthenticationScheme = AuthenticationSchemes.Anonymous;
            http.ProxyAuthenticationScheme = AuthenticationSchemes.Anonymous;
            http.Realm = DefaultRealm;
            //ExtendedProtectionPolicy is always copied - even for security mode None, Message and TransportWithMessageCredential,
            //because the settings for ExtendedProtectionPolicy are always below the <security><transport> element
            http.ExtendedProtectionPolicy = this.extendedProtectionPolicy;
        }

        static bool IsDisabledAuthentication(HttpTransportBindingElement http)
        {
            return http.AuthenticationScheme == AuthenticationSchemes.Anonymous && http.ProxyAuthenticationScheme == AuthenticationSchemes.Anonymous && http.Realm == DefaultRealm;
        }

        internal void ConfigureTransportProtectionAndAuthentication(HttpsTransportBindingElement https)
        {
            ConfigureAuthentication(https);
            https.RequireClientCertificate = (this.clientCredentialType == HttpClientCredentialType.Certificate);
        }

        internal static void ConfigureTransportProtectionAndAuthentication(HttpsTransportBindingElement https, HttpTransportSecurity transportSecurity)
        {
            ConfigureAuthentication(https, transportSecurity);
            if (https.RequireClientCertificate)
                transportSecurity.ClientCredentialType = HttpClientCredentialType.Certificate;
        }

        internal void ConfigureTransportAuthentication(HttpTransportBindingElement http)
        {
            if (this.clientCredentialType == HttpClientCredentialType.Certificate)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.CertificateUnsupportedForHttpTransportCredentialOnly)));
            }
            ConfigureAuthentication(http);
        }

        internal static bool IsConfiguredTransportAuthentication(HttpTransportBindingElement http, HttpTransportSecurity transportSecurity)
        {
            if (HttpClientCredentialTypeHelper.MapToClientCredentialType(http.AuthenticationScheme) == HttpClientCredentialType.Certificate)
                return false;
            ConfigureAuthentication(http, transportSecurity);
            return true;
        }

        internal void DisableTransportAuthentication(HttpTransportBindingElement http)
        {
            DisableAuthentication(http);
        }

        internal static bool IsDisabledTransportAuthentication(HttpTransportBindingElement http)
        {
            return IsDisabledAuthentication(http);
        }

        internal bool InternalShouldSerialize()
        {
            return this.ShouldSerializeClientCredentialType()
                || this.ShouldSerializeProxyCredentialType()
                || this.ShouldSerializeRealm()
                || this.ShouldSerializeExtendedProtectionPolicy();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeClientCredentialType()
        {
            return this.ClientCredentialType != DefaultClientCredentialType;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeProxyCredentialType()
        {
            return this.proxyCredentialType != DefaultProxyCredentialType;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeRealm()
        {
            return this.Realm != DefaultRealm;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeExtendedProtectionPolicy()
        {
            return !ChannelBindingUtility.AreEqual(this.ExtendedProtectionPolicy, ChannelBindingUtility.DefaultPolicy);
        }
    }
}
