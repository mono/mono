//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.Net;
    using System.Net.Security;
    using System.ComponentModel;

    public sealed class TcpTransportSecurity
    {
        internal const TcpClientCredentialType DefaultClientCredentialType = TcpClientCredentialType.Windows;
        internal const ProtectionLevel DefaultProtectionLevel = ProtectionLevel.EncryptAndSign;

        TcpClientCredentialType clientCredentialType;
        ProtectionLevel protectionLevel;
        ExtendedProtectionPolicy extendedProtectionPolicy;

        public TcpTransportSecurity()
        {
            this.clientCredentialType = DefaultClientCredentialType;
            this.protectionLevel = DefaultProtectionLevel;
            this.extendedProtectionPolicy = ChannelBindingUtility.DefaultPolicy;
        }

        [DefaultValue(DefaultClientCredentialType)]
        public TcpClientCredentialType ClientCredentialType
        {
            get { return this.clientCredentialType; }
            set
            {
                if (!TcpClientCredentialTypeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.clientCredentialType = value;
            }
        }

        [DefaultValue(DefaultProtectionLevel)]
        public ProtectionLevel ProtectionLevel
        {
            get { return this.protectionLevel; }
            set
            {
                if (!ProtectionLevelHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.protectionLevel = value;
            }
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

        SslStreamSecurityBindingElement CreateSslBindingElement(bool requireClientCertificate)
        {
            if (this.protectionLevel != ProtectionLevel.EncryptAndSign)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(
                    SR.UnsupportedSslProtectionLevel, this.protectionLevel)));
            }

            SslStreamSecurityBindingElement result = new SslStreamSecurityBindingElement();
            result.RequireClientCertificate = requireClientCertificate;
            return result;
        }

        static bool IsSslBindingElement(BindingElement element, TcpTransportSecurity transportSecurity, out bool requireClientCertificate)
        {
            requireClientCertificate = false;
            SslStreamSecurityBindingElement ssl = element as SslStreamSecurityBindingElement;
            if (ssl == null)
                return false;
            transportSecurity.ProtectionLevel = ProtectionLevel.EncryptAndSign;
            requireClientCertificate = ssl.RequireClientCertificate;
            return true;
        }

        internal BindingElement CreateTransportProtectionOnly()
        {
            return this.CreateSslBindingElement(false);
        }

        internal static bool SetTransportProtectionOnly(BindingElement transport, TcpTransportSecurity transportSecurity)
        {
            bool requireClientCertificate;
            return IsSslBindingElement(transport, transportSecurity, out requireClientCertificate);
        }

        internal BindingElement CreateTransportProtectionAndAuthentication()
        {
            if (this.clientCredentialType == TcpClientCredentialType.Certificate || this.clientCredentialType == TcpClientCredentialType.None)
            {
                return this.CreateSslBindingElement(this.clientCredentialType == TcpClientCredentialType.Certificate);
            }
            else
            {
                WindowsStreamSecurityBindingElement result = new WindowsStreamSecurityBindingElement();
                result.ProtectionLevel = this.protectionLevel;
                return result;
            }
        }

        internal static bool SetTransportProtectionAndAuthentication(BindingElement transport, TcpTransportSecurity transportSecurity)
        {
            bool requireClientCertificate = false;
            if (transport is WindowsStreamSecurityBindingElement)
            {
                transportSecurity.ClientCredentialType = TcpClientCredentialType.Windows;
                transportSecurity.ProtectionLevel = ((WindowsStreamSecurityBindingElement)transport).ProtectionLevel;
                return true;
            }
            else if (IsSslBindingElement(transport, transportSecurity, out requireClientCertificate))
            {
                transportSecurity.ClientCredentialType = requireClientCertificate ? TcpClientCredentialType.Certificate : TcpClientCredentialType.None;
                return true;
            }
            return false;
        }

        internal bool InternalShouldSerialize()
        {
            return this.ClientCredentialType != TcpTransportSecurity.DefaultClientCredentialType
                || this.ProtectionLevel != TcpTransportSecurity.DefaultProtectionLevel
                || ShouldSerializeExtendedProtectionPolicy();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeExtendedProtectionPolicy()
        {
            return !ChannelBindingUtility.AreEqual(this.ExtendedProtectionPolicy, ChannelBindingUtility.DefaultPolicy);
        }
    }
}
