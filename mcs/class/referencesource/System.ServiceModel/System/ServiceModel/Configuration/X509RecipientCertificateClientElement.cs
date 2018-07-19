//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.ServiceModel;
    using System.Configuration;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.Xml;
    using System.Security.Cryptography.X509Certificates;

    public sealed partial class X509RecipientCertificateClientElement : ConfigurationElement
    {
        public X509RecipientCertificateClientElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.DefaultCertificate)]
        public X509DefaultServiceCertificateElement DefaultCertificate
        {
            get { return (X509DefaultServiceCertificateElement)base[ConfigurationStrings.DefaultCertificate]; }
        }

        [ConfigurationProperty(ConfigurationStrings.ScopedCertificates)]
        public X509ScopedServiceCertificateElementCollection ScopedCertificates
        {
            get { return (X509ScopedServiceCertificateElementCollection)base[ConfigurationStrings.ScopedCertificates]; }
        }

        [ConfigurationProperty(ConfigurationStrings.Authentication)]
        public X509ServiceCertificateAuthenticationElement Authentication
        {
            get { return (X509ServiceCertificateAuthenticationElement)base[ConfigurationStrings.Authentication]; }
        }

        [ConfigurationProperty(ConfigurationStrings.SslCertificateAuthentication)]
        public X509ServiceCertificateAuthenticationElement SslCertificateAuthentication
        {
            get { return (X509ServiceCertificateAuthenticationElement)base[ConfigurationStrings.SslCertificateAuthentication]; }
        }

        public void Copy(X509RecipientCertificateClientElement from)
        {
            if (this.IsReadOnly())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigReadOnly)));
            }
            if (null == from)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("from");
            }

            this.DefaultCertificate.Copy(from.DefaultCertificate);

            X509ScopedServiceCertificateElementCollection srcScopedCertificates = from.ScopedCertificates;
            X509ScopedServiceCertificateElementCollection dstScopedCertificates = this.ScopedCertificates;
            dstScopedCertificates.Clear();
            for (int i = 0; i < srcScopedCertificates.Count; ++i)
            {
                dstScopedCertificates.Add(srcScopedCertificates[i]);
            }

            this.Authentication.Copy(from.Authentication);
            this.SslCertificateAuthentication.Copy(from.SslCertificateAuthentication);
        }

        internal void ApplyConfiguration(X509CertificateRecipientClientCredential cert)
        {
            if (cert == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("cert");
            }

            PropertyInformationCollection propertyInfo = this.ElementInformation.Properties;
            if (propertyInfo[ConfigurationStrings.Authentication].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.Authentication.ApplyConfiguration(cert.Authentication);
            }

            if (propertyInfo[ConfigurationStrings.SslCertificateAuthentication].ValueOrigin != PropertyValueOrigin.Default)
            {
                cert.SslCertificateAuthentication = new X509ServiceCertificateAuthentication();
                this.SslCertificateAuthentication.ApplyConfiguration(cert.SslCertificateAuthentication);
            }

            this.DefaultCertificate.ApplyConfiguration(cert);

            X509ScopedServiceCertificateElementCollection scopedCertificates = this.ScopedCertificates;
            for (int i = 0; i < scopedCertificates.Count; ++i)
            {
                scopedCertificates[i].ApplyConfiguration(cert);
            }
        }
    }
}



