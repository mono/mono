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
    using System.IdentityModel.Selectors;

    public sealed partial class X509ServiceCertificateAuthenticationElement : ConfigurationElement
    {
        public X509ServiceCertificateAuthenticationElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.CustomCertificateValidatorType, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string CustomCertificateValidatorType
        {
            get { return (string)base[ConfigurationStrings.CustomCertificateValidatorType]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }
                base[ConfigurationStrings.CustomCertificateValidatorType] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.CertificateValidationMode, DefaultValue = X509ServiceCertificateAuthentication.DefaultCertificateValidationMode)]
        [ServiceModelEnumValidator(typeof(X509CertificateValidationModeHelper))]
        public X509CertificateValidationMode CertificateValidationMode
        {
            get { return (X509CertificateValidationMode)base[ConfigurationStrings.CertificateValidationMode]; }
            set { base[ConfigurationStrings.CertificateValidationMode] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.RevocationMode, DefaultValue = X509ServiceCertificateAuthentication.DefaultRevocationMode)]
        [StandardRuntimeEnumValidator(typeof(X509RevocationMode))]
        public X509RevocationMode RevocationMode
        {
            get { return (X509RevocationMode)base[ConfigurationStrings.RevocationMode]; }
            set { base[ConfigurationStrings.RevocationMode] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.TrustedStoreLocation, DefaultValue = X509ServiceCertificateAuthentication.DefaultTrustedStoreLocation)]
        [StandardRuntimeEnumValidator(typeof(StoreLocation))]
        public StoreLocation TrustedStoreLocation
        {
            get { return (StoreLocation)base[ConfigurationStrings.TrustedStoreLocation]; }
            set { base[ConfigurationStrings.TrustedStoreLocation] = value; }
        }

        public void Copy(X509ServiceCertificateAuthenticationElement from)
        {
            if (this.IsReadOnly())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigReadOnly)));
            }
            if (null == from)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("from");
            }

            this.CertificateValidationMode = from.CertificateValidationMode;
            this.RevocationMode = from.RevocationMode;
            this.TrustedStoreLocation = from.TrustedStoreLocation;
            this.CustomCertificateValidatorType = from.CustomCertificateValidatorType;
        }

        internal void ApplyConfiguration(X509ServiceCertificateAuthentication cert)
        {
            if (cert == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("cert");
            }

            cert.CertificateValidationMode = this.CertificateValidationMode;
            cert.RevocationMode = this.RevocationMode;
            cert.TrustedStoreLocation = this.TrustedStoreLocation;
            if (!string.IsNullOrEmpty(this.CustomCertificateValidatorType))
            {
                Type validatorType = System.Type.GetType(this.CustomCertificateValidatorType, true);
                if (!typeof(X509CertificateValidator).IsAssignableFrom(validatorType))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(
                        SR.GetString(SR.ConfigInvalidCertificateValidatorType, this.CustomCertificateValidatorType, typeof(X509CertificateValidator).ToString())));
                }
                cert.CustomCertificateValidator = (X509CertificateValidator)Activator.CreateInstance(validatorType);
            }
        }
    }
}



