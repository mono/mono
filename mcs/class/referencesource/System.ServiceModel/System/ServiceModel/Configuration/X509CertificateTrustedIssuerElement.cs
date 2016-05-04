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

    public sealed partial class X509CertificateTrustedIssuerElement : ConfigurationElement
    {
        public X509CertificateTrustedIssuerElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.FindValue, DefaultValue = "", Options = ConfigurationPropertyOptions.IsKey)]
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

        [ConfigurationProperty(ConfigurationStrings.StoreLocation, DefaultValue = X509CertificateRecipientServiceCredential.DefaultStoreLocation, Options = ConfigurationPropertyOptions.IsKey)]
        [StandardRuntimeEnumValidator(typeof(StoreLocation))]
        public StoreLocation StoreLocation
        {
            get { return (StoreLocation)base[ConfigurationStrings.StoreLocation]; }
            set { base[ConfigurationStrings.StoreLocation] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.StoreName, DefaultValue = X509CertificateRecipientServiceCredential.DefaultStoreName, Options = ConfigurationPropertyOptions.IsKey)]
        [StandardRuntimeEnumValidator(typeof(StoreName))]
        public StoreName StoreName
        {
            get { return (StoreName)base[ConfigurationStrings.StoreName]; }
            set { base[ConfigurationStrings.StoreName] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.X509FindType, DefaultValue = X509CertificateRecipientServiceCredential.DefaultFindType, Options = ConfigurationPropertyOptions.IsKey)]
        [StandardRuntimeEnumValidator(typeof(X509FindType))]
        public X509FindType X509FindType
        {
            get { return (X509FindType)base[ConfigurationStrings.X509FindType]; }
            set { base[ConfigurationStrings.X509FindType] = value; }
        }

        public void Copy(X509CertificateTrustedIssuerElement from)
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
    }
}



