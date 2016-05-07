//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;

    [ConfigurationCollection(typeof(X509CertificateTrustedIssuerElement))]
    public sealed class X509CertificateTrustedIssuerElementCollection : ServiceModelConfigurationElementCollection<X509CertificateTrustedIssuerElement>
    {
        public X509CertificateTrustedIssuerElementCollection()
            : base()
        { }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return element;
        }
    }
}
