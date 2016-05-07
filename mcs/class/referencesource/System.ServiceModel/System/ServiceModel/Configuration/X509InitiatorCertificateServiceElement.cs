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

    public sealed partial class X509InitiatorCertificateServiceElement : ConfigurationElement
    {
        public X509InitiatorCertificateServiceElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.Certificate)]
        public X509ClientCertificateCredentialsElement Certificate
        {
            get { return (X509ClientCertificateCredentialsElement)base[ConfigurationStrings.Certificate]; }
        }

        [ConfigurationProperty(ConfigurationStrings.Authentication)]
        public X509ClientCertificateAuthenticationElement Authentication
        {
            get { return (X509ClientCertificateAuthenticationElement)base[ConfigurationStrings.Authentication]; }
        }

        public void Copy(X509InitiatorCertificateServiceElement from)
        {
            if (this.IsReadOnly())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigReadOnly)));
            }
            if (null == from)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("from");
            }

            this.Authentication.Copy(from.Authentication);
            this.Certificate.Copy(from.Certificate);
        }

        internal void ApplyConfiguration(X509CertificateInitiatorServiceCredential cert)
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
            if (propertyInfo[ConfigurationStrings.Certificate].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.Certificate.ApplyConfiguration(cert);
            }
        }
    }
}



