//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.Xml;
    using System.Security.Cryptography.X509Certificates;

    public sealed partial class PeerCredentialElement : ConfigurationElement
    {
        public PeerCredentialElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.Certificate)]
        public X509PeerCertificateElement Certificate
        {
            get { return (X509PeerCertificateElement)base[ConfigurationStrings.Certificate]; }
        }

        [ConfigurationProperty(ConfigurationStrings.PeerAuthentication)]
        public X509PeerCertificateAuthenticationElement PeerAuthentication
        {
            get { return (X509PeerCertificateAuthenticationElement)base[ConfigurationStrings.PeerAuthentication]; }
        }

        [ConfigurationProperty(ConfigurationStrings.MessageSenderAuthentication)]
        public X509PeerCertificateAuthenticationElement MessageSenderAuthentication
        {
            get { return (X509PeerCertificateAuthenticationElement)base[ConfigurationStrings.MessageSenderAuthentication]; }
        }

        public void Copy(PeerCredentialElement from)
        {
            if (this.IsReadOnly())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigReadOnly)));
            }
            if (null == from)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("from");
            }

            this.Certificate.Copy(from.Certificate);
            this.PeerAuthentication.Copy(from.PeerAuthentication);
            this.MessageSenderAuthentication.Copy(from.MessageSenderAuthentication);
        }

        internal void ApplyConfiguration(PeerCredential creds)
        {
            if (creds == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("creds");
            }

            PropertyInformationCollection propertyInfo = this.ElementInformation.Properties;
            if (propertyInfo[ConfigurationStrings.Certificate].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.Certificate.ApplyConfiguration(creds);
            }
            if (propertyInfo[ConfigurationStrings.PeerAuthentication].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.PeerAuthentication.ApplyConfiguration(creds.PeerAuthentication);
            }
            if (propertyInfo[ConfigurationStrings.MessageSenderAuthentication].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.MessageSenderAuthentication.ApplyConfiguration(creds.MessageSenderAuthentication);
            }
        }
    }
}



