//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.ServiceModel;
    using System.Configuration;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Xml;

    public sealed partial class IdentityElement : ConfigurationElement
    {
        public IdentityElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.UserPrincipalName)]
        public UserPrincipalNameElement UserPrincipalName
        {
            get { return (UserPrincipalNameElement)base[ConfigurationStrings.UserPrincipalName]; }
        }

        [ConfigurationProperty(ConfigurationStrings.ServicePrincipalName)]
        public ServicePrincipalNameElement ServicePrincipalName
        {
            get { return (ServicePrincipalNameElement)base[ConfigurationStrings.ServicePrincipalName]; }
        }

        [ConfigurationProperty(ConfigurationStrings.Dns)]
        public DnsElement Dns
        {
            get { return (DnsElement)base[ConfigurationStrings.Dns]; }
        }

        [ConfigurationProperty(ConfigurationStrings.Rsa)]
        public RsaElement Rsa
        {
            get { return (RsaElement)base[ConfigurationStrings.Rsa]; }
        }

        [ConfigurationProperty(ConfigurationStrings.Certificate)]
        public CertificateElement Certificate
        {
            get { return (CertificateElement)base[ConfigurationStrings.Certificate]; }
        }

        [ConfigurationProperty(ConfigurationStrings.CertificateReference)]
        public CertificateReferenceElement CertificateReference
        {
            get { return (CertificateReferenceElement)base[ConfigurationStrings.CertificateReference]; }
        }

        internal void Copy(IdentityElement source)
        {
            if (null == source)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("source");
            }

            PropertyInformationCollection properties = source.ElementInformation.Properties;
            if (properties[ConfigurationStrings.UserPrincipalName].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.UserPrincipalName.Value = source.UserPrincipalName.Value;
            }
            if (properties[ConfigurationStrings.ServicePrincipalName].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.ServicePrincipalName.Value = source.ServicePrincipalName.Value;
            }
            if (properties[ConfigurationStrings.Certificate].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.Certificate.EncodedValue = source.Certificate.EncodedValue;
            }
            if (properties[ConfigurationStrings.CertificateReference].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.CertificateReference.StoreName = source.CertificateReference.StoreName;
                this.CertificateReference.StoreLocation = source.CertificateReference.StoreLocation;
                this.CertificateReference.X509FindType = source.CertificateReference.X509FindType;
                this.CertificateReference.FindValue = source.CertificateReference.FindValue;
            }
        }

        public void InitializeFrom(EndpointIdentity identity)
        {
            if (identity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("identity");
            }

            Claim claim = identity.IdentityClaim;
            if (ClaimTypes.Dns.Equals(claim.ClaimType))
            {
                this.Dns.Value = (string)claim.Resource;
            }
            else if (ClaimTypes.Spn.Equals(claim.ClaimType))
            {
                this.ServicePrincipalName.Value = (string)claim.Resource;
            }
            else if (ClaimTypes.Upn.Equals(claim.ClaimType))
            {
                this.UserPrincipalName.Value = (string)claim.Resource;
            }
            else if (ClaimTypes.Rsa.Equals(claim.ClaimType))
            {
                this.Rsa.Value = ((RSA)claim.Resource).ToXmlString(false);
            }
            else if (identity is X509CertificateEndpointIdentity)
            {
                X509Certificate2Collection certs = ((X509CertificateEndpointIdentity)identity).Certificates;
#pragma warning suppress 56506 //Microsoft; this.Certificate can never be null (underlying configuration system guarantees)
                this.Certificate.EncodedValue = Convert.ToBase64String(certs.Export(certs.Count == 1 ? X509ContentType.SerializedCert : X509ContentType.SerializedStore));
            }
        }
    }
}
