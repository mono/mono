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

    public sealed partial class X509DefaultServiceCertificateElement : ConfigurationElement
    {
        public X509DefaultServiceCertificateElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.FindValue, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string FindValue
        {
            get { return (string)base[ConfigurationStrings.FindValue]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }
                base[ConfigurationStrings.FindValue] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.StoreLocation, DefaultValue = X509CertificateRecipientClientCredential.DefaultStoreLocation)]
        [StandardRuntimeEnumValidator(typeof(StoreLocation))]
        public StoreLocation StoreLocation
        {
            get { return (StoreLocation)base[ConfigurationStrings.StoreLocation]; }
            set { base[ConfigurationStrings.StoreLocation] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.StoreName, DefaultValue = X509CertificateRecipientClientCredential.DefaultStoreName)]
        [StandardRuntimeEnumValidator(typeof(StoreName))]
        public StoreName StoreName
        {
            get { return (StoreName)base[ConfigurationStrings.StoreName]; }
            set { base[ConfigurationStrings.StoreName] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.X509FindType, DefaultValue = X509CertificateRecipientClientCredential.DefaultFindType)]
        [StandardRuntimeEnumValidator(typeof(X509FindType))]
        public X509FindType X509FindType
        {
            get { return (X509FindType)base[ConfigurationStrings.X509FindType]; }
            set { base[ConfigurationStrings.X509FindType] = value; }
        }

        public void Copy(X509DefaultServiceCertificateElement from)
        {
            if (this.IsReadOnly())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigReadOnly)));
            }
            if (null == from)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("from");
            }

            this.FindValue = from.FindValue;
            this.StoreLocation = from.StoreLocation;
            this.StoreName = from.StoreName;
            this.X509FindType = from.X509FindType;
        }

        internal void ApplyConfiguration(X509CertificateRecipientClientCredential creds)
        {
            if (creds == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("creds");
            }
            PropertyInformationCollection propertyInfo = this.ElementInformation.Properties;
            if (propertyInfo[ConfigurationStrings.StoreLocation].ValueOrigin != PropertyValueOrigin.Default
                || propertyInfo[ConfigurationStrings.StoreName].ValueOrigin != PropertyValueOrigin.Default
                || propertyInfo[ConfigurationStrings.X509FindType].ValueOrigin != PropertyValueOrigin.Default
                || propertyInfo[ConfigurationStrings.FindValue].ValueOrigin != PropertyValueOrigin.Default)
            {
                creds.SetDefaultCertificate(this.StoreLocation, this.StoreName, this.X509FindType, this.FindValue);
            }
        }
    }
}



