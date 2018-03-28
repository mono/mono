//------------------------------------------------------------------------------
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

using System.ComponentModel;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security;

namespace System.IdentityModel.Configuration
{
    /// <summary>
    /// Manages the configuration of a X509CertificateValidation element in IdentityConfiguration.
    /// </summary>
    public sealed partial class X509CertificateValidationElement : ConfigurationElement
    {
        const X509CertificateValidationMode DefaultX509CertificateValidationMode = X509CertificateValidationMode.PeerOrChainTrust;
        const X509RevocationMode DefaultX509RevocationMode = X509RevocationMode.Online;
        const StoreLocation DefaultStoreLocation = StoreLocation.LocalMachine;

        /// <summary>
        /// Mode, optional.  Default is PeerOrChainTrust.
        /// </summary>
        [ConfigurationProperty( ConfigurationStrings.X509CertificateValidationMode, IsRequired = false, DefaultValue = DefaultX509CertificateValidationMode )]
        [StandardRuntimeEnumValidator( typeof( X509CertificateValidationMode ) )]
        public X509CertificateValidationMode CertificateValidationMode
        {
            get { return ( X509CertificateValidationMode ) this[ConfigurationStrings.X509CertificateValidationMode]; }
            set { this[ConfigurationStrings.X509CertificateValidationMode] = value; }
        }

        /// <summary>
        /// X509RevocationMode, optional.  Default is Online.
        /// </summary>
        [ConfigurationProperty( ConfigurationStrings.X509CertificateRevocationMode, IsRequired = false, DefaultValue = DefaultX509RevocationMode )]
        [StandardRuntimeEnumValidator( typeof( X509RevocationMode ) )]
        public X509RevocationMode RevocationMode
        {
            get { return ( X509RevocationMode ) this[ConfigurationStrings.X509CertificateRevocationMode]; }
            set { this[ConfigurationStrings.X509CertificateRevocationMode] = value; }
        }

        /// <summary>
        /// TrustedStoreLocation, optional.  Default is LocalMachine.
        /// </summary>
        [ConfigurationProperty( ConfigurationStrings.X509TrustedStoreLocation, IsRequired = false, DefaultValue = DefaultStoreLocation )]
        [StandardRuntimeEnumValidator( typeof( StoreLocation ) )]
        public StoreLocation TrustedStoreLocation
        {
            get { return ( StoreLocation ) this[ConfigurationStrings.X509TrustedStoreLocation]; }
            set { this[ConfigurationStrings.X509TrustedStoreLocation] = value; }
        }

        /// <summary>
        /// CertificateValidator type, optional.
        /// </summary>
        [ConfigurationProperty( ConfigurationStrings.X509CertificateValidator, IsRequired = false )]
        public CustomTypeElement CertificateValidator
        {
            get { return ( CustomTypeElement ) this[ConfigurationStrings.X509CertificateValidator]; }
            set { this[ConfigurationStrings.X509CertificateValidator] = value; }
        }

        /// <summary>
        /// Returns a value indicating whether this element has been configured with non-default values.
        /// </summary>
        internal bool IsConfigured
        {
            get
            {
                return (
                    ( ElementInformation.Properties[ConfigurationStrings.X509CertificateValidationMode].ValueOrigin != PropertyValueOrigin.Default ) ||
                    ( ElementInformation.Properties[ConfigurationStrings.X509CertificateRevocationMode].ValueOrigin != PropertyValueOrigin.Default ) ||
                    ( ElementInformation.Properties[ConfigurationStrings.X509TrustedStoreLocation].ValueOrigin != PropertyValueOrigin.Default ) ||
                    CertificateValidator.IsConfigured);
            }
        }
    }
}
