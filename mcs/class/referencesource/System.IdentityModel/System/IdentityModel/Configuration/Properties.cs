//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

// This code was produced by a tool, ConfigPropertyGenerator.exe, by reflecting over
// System.IdentityModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089.
// Please add this file to the project that built the assembly.
// Doing so will provide better performance for retrieving the ConfigurationElement Properties.
// If compilation errors occur, make sure that the Properties property has not
// already been provided. If it has, decide if you want the version produced by 
// this tool or by the developer.
// If build errors result, make sure the config class is marked with the partial keyword.

// To regenerate a new Properties.cs after changes to the configuration OM for
// this assembly, simply run Indigo\Suites\Configuration\Infrastructure\ConfigPropertyGenerator.
// If any changes affect this file, the suite will fail.  Instructions on how to
// update Properties.cs will be included in the tests output file (ConfigPropertyGenerator.out).

using System.Configuration;
using System.Globalization;


// configType.Name: AudienceUriElement

namespace System.IdentityModel.Configuration
{
    public sealed partial class AudienceUriElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("value", typeof(System.String), " ", null, new System.Configuration.StringValidator(1, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsRequired | System.Configuration.ConfigurationPropertyOptions.IsKey));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: AudienceUriElementCollection

namespace System.IdentityModel.Configuration
{
    public sealed partial class AudienceUriElementCollection
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("mode", typeof(System.IdentityModel.Selectors.AudienceUriMode), System.IdentityModel.Selectors.AudienceUriMode.Always, null, new System.IdentityModel.Configuration.StandardRuntimeEnumValidator(typeof(System.IdentityModel.Selectors.AudienceUriMode)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: CustomTypeElement

namespace System.IdentityModel.Configuration
{
    public sealed partial class CustomTypeElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("type", typeof(System.Type), null, new System.Configuration.TypeNameConverter(), null, System.Configuration.ConfigurationPropertyOptions.IsRequired | System.Configuration.ConfigurationPropertyOptions.IsKey));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: IdentityModelCachesElement

namespace System.IdentityModel.Configuration
{
    public sealed partial class IdentityModelCachesElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("tokenReplayCache", typeof(System.IdentityModel.Configuration.CustomTypeElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("sessionSecurityTokenCache", typeof(System.IdentityModel.Configuration.CustomTypeElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: IssuerNameRegistryElement

namespace System.IdentityModel.Configuration
{
    public sealed partial class IssuerNameRegistryElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("type", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: X509CertificateValidationElement

namespace System.IdentityModel.Configuration
{
    public sealed partial class X509CertificateValidationElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("certificateValidationMode", typeof(System.ServiceModel.Security.X509CertificateValidationMode), System.ServiceModel.Security.X509CertificateValidationMode.PeerOrChainTrust, null, new System.IdentityModel.Configuration.StandardRuntimeEnumValidator(typeof(System.ServiceModel.Security.X509CertificateValidationMode)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("revocationMode", typeof(System.Security.Cryptography.X509Certificates.X509RevocationMode), System.Security.Cryptography.X509Certificates.X509RevocationMode.Online, null, new System.IdentityModel.Configuration.StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.X509RevocationMode)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("trustedStoreLocation", typeof(System.Security.Cryptography.X509Certificates.StoreLocation), System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine, null, new System.IdentityModel.Configuration.StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.StoreLocation)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("certificateValidator", typeof(System.IdentityModel.Configuration.CustomTypeElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: SecurityTokenHandlerConfigurationElement

namespace System.IdentityModel.Configuration
{
    public sealed partial class SecurityTokenHandlerConfigurationElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("audienceUris", typeof(System.IdentityModel.Configuration.AudienceUriElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("caches", typeof(System.IdentityModel.Configuration.IdentityModelCachesElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("certificateValidation", typeof(System.IdentityModel.Configuration.X509CertificateValidationElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("issuerNameRegistry", typeof(System.IdentityModel.Configuration.IssuerNameRegistryElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("issuerTokenResolver", typeof(System.IdentityModel.Configuration.CustomTypeElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("name", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsKey));
                    properties.Add(new ConfigurationProperty("saveBootstrapContext", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maximumClockSkew", typeof(System.TimeSpan), System.TimeSpan.Parse("00:05:00", CultureInfo.InvariantCulture), new System.IdentityModel.Configuration.TimeSpanOrInfiniteConverter(), new System.IdentityModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("serviceTokenResolver", typeof(System.IdentityModel.Configuration.CustomTypeElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("tokenReplayDetection", typeof(System.IdentityModel.Configuration.TokenReplayDetectionElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: SecurityTokenHandlerElementCollection

namespace System.IdentityModel.Configuration
{
    public sealed partial class SecurityTokenHandlerElementCollection
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("name", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsKey));
                    properties.Add(new ConfigurationProperty("securityTokenHandlerConfiguration", typeof(System.IdentityModel.Configuration.SecurityTokenHandlerConfigurationElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: TokenReplayDetectionElement

namespace System.IdentityModel.Configuration
{
    public sealed partial class TokenReplayDetectionElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("enabled", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("expirationPeriod", typeof(System.TimeSpan), System.TimeSpan.Parse("10675199.02:48:05.4775807", CultureInfo.InvariantCulture), new System.IdentityModel.Configuration.TimeSpanOrInfiniteConverter(), new System.IdentityModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("10675199.02:48:05.4775807", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

