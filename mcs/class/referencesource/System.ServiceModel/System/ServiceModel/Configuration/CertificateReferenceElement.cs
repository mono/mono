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

    public sealed partial class CertificateReferenceElement : ConfigurationElement
    {
        public CertificateReferenceElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.StoreName, DefaultValue = EndpointIdentity.defaultStoreName)]
        [StandardRuntimeEnumValidator(typeof(StoreName))]
        public StoreName StoreName
        {
            get { return (StoreName)base[ConfigurationStrings.StoreName]; }
            set { base[ConfigurationStrings.StoreName] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.StoreLocation, DefaultValue = EndpointIdentity.defaultStoreLocation)]
        [StandardRuntimeEnumValidator(typeof(StoreLocation))]
        public StoreLocation StoreLocation
        {
            get { return (StoreLocation)base[ConfigurationStrings.StoreLocation]; }
            set { base[ConfigurationStrings.StoreLocation] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.X509FindType, DefaultValue = EndpointIdentity.defaultX509FindType)]
        [StandardRuntimeEnumValidator(typeof(X509FindType))]
        public X509FindType X509FindType
        {
            get { return (X509FindType)base[ConfigurationStrings.X509FindType]; }
            set { base[ConfigurationStrings.X509FindType] = value; }
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

        [ConfigurationProperty(ConfigurationStrings.IsChainIncluded, DefaultValue = false)]
        public bool IsChainIncluded
        {
            get { return (bool)base[ConfigurationStrings.IsChainIncluded]; }
            set { base[ConfigurationStrings.IsChainIncluded] = value; }
        }
    }

}
