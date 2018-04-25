//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

// This code was produced by a tool, ConfigPropertyGenerator.exe, by reflecting over
// System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089.
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


// configType.Name: DiagnosticSection

namespace System.ServiceModel.Activation.Configuration
{
    public sealed partial class DiagnosticSection
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("performanceCountersEnabled", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: NetPipeSection

namespace System.ServiceModel.Activation.Configuration
{
    public sealed partial class NetPipeSection
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("allowAccounts", typeof(System.ServiceModel.Activation.Configuration.SecurityIdentifierElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxPendingConnections", typeof(System.Int32), 100, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxPendingAccepts", typeof(System.Int32), 0, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("receiveTimeout", typeof(System.TimeSpan), System.TimeSpan.Parse("00:00:30", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: NetTcpSection

namespace System.ServiceModel.Activation.Configuration
{
    public sealed partial class NetTcpSection
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("allowAccounts", typeof(System.ServiceModel.Activation.Configuration.SecurityIdentifierElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("listenBacklog", typeof(System.Int32), 0, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxPendingConnections", typeof(System.Int32), 100, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxPendingAccepts", typeof(System.Int32), 0, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("receiveTimeout", typeof(System.TimeSpan), System.TimeSpan.Parse("00:00:30", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("teredoEnabled", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: SecurityIdentifierElement

namespace System.ServiceModel.Activation.Configuration
{
    public sealed partial class SecurityIdentifierElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("securityIdentifier", typeof(System.Security.Principal.SecurityIdentifier), null, new System.ServiceModel.Activation.Configuration.SecurityIdentifierConverter(), null, System.Configuration.ConfigurationPropertyOptions.IsKey));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: ContextBindingElementExtensionElement

namespace System.ServiceModel.Configuration
{
    public partial class ContextBindingElementExtensionElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("clientCallbackAddress", typeof(System.Uri), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("contextExchangeMechanism", typeof(System.ServiceModel.Channels.ContextExchangeMechanism), System.ServiceModel.Channels.ContextExchangeMechanism.ContextSoapHeader, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.Channels.ContextExchangeMechanismHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("protectionLevel", typeof(System.Net.Security.ProtectionLevel), System.Net.Security.ProtectionLevel.Sign, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.Security.ProtectionLevelHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("contextManagementEnabled", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: NetTcpContextBindingElement

namespace System.ServiceModel.Configuration
{
    public partial class NetTcpContextBindingElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("clientCallbackAddress", typeof(System.Uri), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("contextManagementEnabled", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("contextProtectionLevel", typeof(System.Net.Security.ProtectionLevel), System.Net.Security.ProtectionLevel.Sign, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.Security.ProtectionLevelHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: WSHttpContextBindingElement

namespace System.ServiceModel.Configuration
{
    public partial class WSHttpContextBindingElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("clientCallbackAddress", typeof(System.Uri), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("contextManagementEnabled", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("contextProtectionLevel", typeof(System.Net.Security.ProtectionLevel), System.Net.Security.ProtectionLevel.Sign, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.Security.ProtectionLevelHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: BasicHttpContextBindingElement

namespace System.ServiceModel.Configuration
{
    public partial class BasicHttpContextBindingElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("contextManagementEnabled", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: ApplicationContainerSettingsElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class ApplicationContainerSettingsElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("packageFullName", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("sessionId", typeof(System.Int32), "CurrentSession", new System.ServiceModel.Configuration.SessionIdTypeConvertor(), new System.ServiceModel.Configuration.ApplicationContainerSettingsElement.SessionIdTypeValidator(), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: AllowedAudienceUriElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class AllowedAudienceUriElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("allowedAudienceUri", typeof(System.String), null, null, new System.Configuration.StringValidator(1, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsKey));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: AddressHeaderCollectionElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class AddressHeaderCollectionElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("headers", typeof(System.ServiceModel.Channels.AddressHeaderCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: ServiceAuthenticationElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class ServiceAuthenticationElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("serviceAuthenticationManagerType", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("authenticationSchemes", typeof(System.Net.AuthenticationSchemes), System.Net.AuthenticationSchemes.None, null, new System.ServiceModel.Configuration.StandardRuntimeFlagEnumValidator<System.Net.AuthenticationSchemes>(), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: AuthorizationPolicyTypeElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class AuthorizationPolicyTypeElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("policyType", typeof(System.String), null, null, new System.Configuration.StringValidator(1, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsRequired | System.Configuration.ConfigurationPropertyOptions.IsKey));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: BaseAddressElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class BaseAddressElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("baseAddress", typeof(System.String), null, null, new System.Configuration.StringValidator(1, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsRequired | System.Configuration.ConfigurationPropertyOptions.IsKey));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: BaseAddressPrefixFilterElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class BaseAddressPrefixFilterElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("prefix", typeof(System.Uri), null, null, null, System.Configuration.ConfigurationPropertyOptions.IsRequired | System.Configuration.ConfigurationPropertyOptions.IsKey));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: BasicHttpBindingElement

namespace System.ServiceModel.Configuration
{
    public partial class BasicHttpBindingElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("messageEncoding", typeof(System.ServiceModel.WSMessageEncoding), System.ServiceModel.WSMessageEncoding.Text, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.WSMessageEncodingHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("security", typeof(System.ServiceModel.Configuration.BasicHttpSecurityElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: BasicHttpMessageSecurityElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class BasicHttpMessageSecurityElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("clientCredentialType", typeof(System.ServiceModel.BasicHttpMessageCredentialType), System.ServiceModel.BasicHttpMessageCredentialType.UserName, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.BasicHttpMessageCredentialTypeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("algorithmSuite", typeof(System.ServiceModel.Security.SecurityAlgorithmSuite), "Default", new System.ServiceModel.Configuration.SecurityAlgorithmSuiteConverter(), null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: BasicHttpSecurityElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class BasicHttpSecurityElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("mode", typeof(System.ServiceModel.BasicHttpSecurityMode), System.ServiceModel.BasicHttpSecurityMode.None, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.BasicHttpSecurityModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("transport", typeof(System.ServiceModel.Configuration.HttpTransportSecurityElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("message", typeof(System.ServiceModel.Configuration.BasicHttpMessageSecurityElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: BasicHttpsBindingElement

namespace System.ServiceModel.Configuration
{
    public partial class BasicHttpsBindingElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("messageEncoding", typeof(System.ServiceModel.WSMessageEncoding), System.ServiceModel.WSMessageEncoding.Text, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.WSMessageEncodingHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("security", typeof(System.ServiceModel.Configuration.BasicHttpsSecurityElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: BasicHttpsSecurityElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class BasicHttpsSecurityElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("mode", typeof(System.ServiceModel.BasicHttpsSecurityMode), System.ServiceModel.BasicHttpsSecurityMode.Transport, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.BasicHttpsSecurityModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("transport", typeof(System.ServiceModel.Configuration.HttpTransportSecurityElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("message", typeof(System.ServiceModel.Configuration.BasicHttpMessageSecurityElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: BehaviorsSection

namespace System.ServiceModel.Configuration
{
    public partial class BehaviorsSection
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("endpointBehaviors", typeof(System.ServiceModel.Configuration.EndpointBehaviorElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("serviceBehaviors", typeof(System.ServiceModel.Configuration.ServiceBehaviorElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: BinaryMessageEncodingElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class BinaryMessageEncodingElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("maxReadPoolSize", typeof(System.Int32), 64, null, new System.Configuration.IntegerValidator(1, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxWritePoolSize", typeof(System.Int32), 16, null, new System.Configuration.IntegerValidator(1, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxSessionSize", typeof(System.Int32), 2048, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("readerQuotas", typeof(System.ServiceModel.Configuration.XmlDictionaryReaderQuotasElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("compressionFormat", typeof(System.ServiceModel.Channels.CompressionFormat), System.ServiceModel.Channels.CompressionFormat.None, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.Channels.CompressionFormatHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: CertificateElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class CertificateElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("encodedValue", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: CertificateReferenceElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class CertificateReferenceElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("storeName", typeof(System.Security.Cryptography.X509Certificates.StoreName), System.Security.Cryptography.X509Certificates.StoreName.My, null, new System.ServiceModel.Configuration.StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.StoreName)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("storeLocation", typeof(System.Security.Cryptography.X509Certificates.StoreLocation), System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine, null, new System.ServiceModel.Configuration.StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.StoreLocation)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("x509FindType", typeof(System.Security.Cryptography.X509Certificates.X509FindType), System.Security.Cryptography.X509Certificates.X509FindType.FindBySubjectDistinguishedName, null, new System.ServiceModel.Configuration.StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.X509FindType)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("findValue", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("isChainIncluded", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: ChannelEndpointElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class ChannelEndpointElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("address", typeof(System.Uri), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("behaviorConfiguration", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("binding", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("bindingConfiguration", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("contract", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsKey));
                    properties.Add(new ConfigurationProperty("headers", typeof(System.ServiceModel.Configuration.AddressHeaderCollectionElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("identity", typeof(System.ServiceModel.Configuration.IdentityElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("name", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsKey));
                    properties.Add(new ConfigurationProperty("kind", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("endpointConfiguration", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: ChannelPoolSettingsElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class ChannelPoolSettingsElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("idleTimeout", typeof(System.TimeSpan), System.TimeSpan.Parse("00:02:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("leaseTimeout", typeof(System.TimeSpan), System.TimeSpan.Parse("00:10:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxOutboundChannelsPerEndpoint", typeof(System.Int32), 10, null, new System.Configuration.IntegerValidator(1, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: ClientViaElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class ClientViaElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("viaUri", typeof(System.Uri), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: ClaimTypeElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class ClaimTypeElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("claimType", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsRequired | System.Configuration.ConfigurationPropertyOptions.IsKey));
                    properties.Add(new ConfigurationProperty("isOptional", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: CallbackDebugElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class CallbackDebugElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("includeExceptionDetailInFaults", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: ClientSection

namespace System.ServiceModel.Configuration
{
    public sealed partial class ClientSection
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("", typeof(System.ServiceModel.Configuration.ChannelEndpointElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.IsDefaultCollection));
                    properties.Add(new ConfigurationProperty("metadata", typeof(System.ServiceModel.Configuration.MetadataElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: CallbackTimeoutsElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class CallbackTimeoutsElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("transactionTimeout", typeof(System.TimeSpan), System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: ClientCredentialsElement

namespace System.ServiceModel.Configuration
{
    public partial class ClientCredentialsElement
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
                    properties.Add(new ConfigurationProperty("useIdentityConfiguration", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("clientCertificate", typeof(System.ServiceModel.Configuration.X509InitiatorCertificateClientElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("serviceCertificate", typeof(System.ServiceModel.Configuration.X509RecipientCertificateClientElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("windows", typeof(System.ServiceModel.Configuration.WindowsClientElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("issuedToken", typeof(System.ServiceModel.Configuration.IssuedTokenClientElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("httpDigest", typeof(System.ServiceModel.Configuration.HttpDigestClientElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("peer", typeof(System.ServiceModel.Configuration.PeerCredentialElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("supportInteractive", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: CommonBehaviorsSection

namespace System.ServiceModel.Configuration
{
    public partial class CommonBehaviorsSection
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("endpointBehaviors", typeof(System.ServiceModel.Configuration.CommonEndpointBehaviorElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("serviceBehaviors", typeof(System.ServiceModel.Configuration.CommonServiceBehaviorElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: CompositeDuplexElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class CompositeDuplexElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("clientBaseAddress", typeof(System.Uri), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: ConnectionOrientedTransportElement

namespace System.ServiceModel.Configuration
{
    public partial class ConnectionOrientedTransportElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("connectionBufferSize", typeof(System.Int32), 8192, null, new System.Configuration.IntegerValidator(1, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("hostNameComparisonMode", typeof(System.ServiceModel.HostNameComparisonMode), System.ServiceModel.HostNameComparisonMode.StrongWildcard, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.HostNameComparisonModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("channelInitializationTimeout", typeof(System.TimeSpan), System.TimeSpan.Parse("00:00:30", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00.0000001", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxBufferSize", typeof(System.Int32), 65536, null, new System.Configuration.IntegerValidator(1, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxPendingConnections", typeof(System.Int32), 0, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxOutputDelay", typeof(System.TimeSpan), System.TimeSpan.Parse("00:00:00.2", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxPendingAccepts", typeof(System.Int32), 0, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("transferMode", typeof(System.ServiceModel.TransferMode), System.ServiceModel.TransferMode.Buffered, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.TransferModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: CustomBindingElement

namespace System.ServiceModel.Configuration
{
    public partial class CustomBindingElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("closeTimeout", typeof(System.TimeSpan), System.TimeSpan.Parse("00:01:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("openTimeout", typeof(System.TimeSpan), System.TimeSpan.Parse("00:01:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("receiveTimeout", typeof(System.TimeSpan), System.TimeSpan.Parse("00:10:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("sendTimeout", typeof(System.TimeSpan), System.TimeSpan.Parse("00:01:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: CustomBindingCollectionElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class CustomBindingCollectionElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("", typeof(System.ServiceModel.Configuration.CustomBindingElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.IsDefaultCollection));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: OneWayElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class OneWayElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("channelPoolSettings", typeof(System.ServiceModel.Configuration.ChannelPoolSettingsElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxAcceptedChannels", typeof(System.Int32), 10, null, new System.Configuration.IntegerValidator(1, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("packetRoutable", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: DefaultPortElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class DefaultPortElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("scheme", typeof(System.String), null, null, new System.Configuration.StringValidator(1, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsRequired | System.Configuration.ConfigurationPropertyOptions.IsKey));
                    properties.Add(new ConfigurationProperty("port", typeof(System.Int32), 0, null, new System.Configuration.IntegerValidator(0, 65535, false), System.Configuration.ConfigurationPropertyOptions.IsRequired));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: DelegatingHandlerElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class DelegatingHandlerElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("type", typeof(System.String), null, null, new System.Configuration.StringValidator(1, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsRequired | System.Configuration.ConfigurationPropertyOptions.IsKey));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: DiagnosticSection

namespace System.ServiceModel.Configuration
{
    public sealed partial class DiagnosticSection
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("wmiProviderEnabled", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("messageLogging", typeof(System.ServiceModel.Configuration.MessageLoggingElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("endToEndTracing", typeof(System.ServiceModel.Configuration.EndToEndTracingElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("performanceCounters", typeof(System.ServiceModel.Diagnostics.PerformanceCounterScope), System.ServiceModel.Diagnostics.PerformanceCounterScope.Default, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.Diagnostics.PerformanceCounterScopeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("etwProviderId", typeof(System.String), "{c651f5f6-1c0d-492e-8ae1-b4efd7c9d503}", null, new System.Configuration.StringValidator(32, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: DnsElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class DnsElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("value", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: EndpointAddressElementBase

namespace System.ServiceModel.Configuration
{
    public partial class EndpointAddressElementBase
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("address", typeof(System.Uri), null, null, null, System.Configuration.ConfigurationPropertyOptions.IsRequired));
                    properties.Add(new ConfigurationProperty("headers", typeof(System.ServiceModel.Configuration.AddressHeaderCollectionElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("identity", typeof(System.ServiceModel.Configuration.IdentityElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: EndToEndTracingElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class EndToEndTracingElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("propagateActivity", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("activityTracing", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("messageFlowTracing", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: FederatedMessageSecurityOverHttpElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class FederatedMessageSecurityOverHttpElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("algorithmSuite", typeof(System.ServiceModel.Security.SecurityAlgorithmSuite), "Default", new System.ServiceModel.Configuration.SecurityAlgorithmSuiteConverter(), null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("claimTypeRequirements", typeof(System.ServiceModel.Configuration.ClaimTypeElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("establishSecurityContext", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("issuedKeyType", typeof(System.IdentityModel.Tokens.SecurityKeyType), System.IdentityModel.Tokens.SecurityKeyType.SymmetricKey, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.IdentityModel.Tokens.SecurityKeyTypeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("issuedTokenType", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("issuer", typeof(System.ServiceModel.Configuration.IssuedTokenParametersEndpointAddressElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("issuerMetadata", typeof(System.ServiceModel.Configuration.EndpointAddressElementBase), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("negotiateServiceCredential", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("tokenRequestParameters", typeof(System.ServiceModel.Configuration.XmlElementElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: HostElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class HostElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("baseAddresses", typeof(System.ServiceModel.Configuration.BaseAddressElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("timeouts", typeof(System.ServiceModel.Configuration.HostTimeoutsElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: HostTimeoutsElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class HostTimeoutsElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("closeTimeout", typeof(System.TimeSpan), System.TimeSpan.Parse("00:00:10", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("openTimeout", typeof(System.TimeSpan), System.TimeSpan.Parse("00:01:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: HttpBindingBaseElement

namespace System.ServiceModel.Configuration
{
    public partial class HttpBindingBaseElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("allowCookies", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("bypassProxyOnLocal", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("hostNameComparisonMode", typeof(System.ServiceModel.HostNameComparisonMode), System.ServiceModel.HostNameComparisonMode.StrongWildcard, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.HostNameComparisonModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxBufferPoolSize", typeof(System.Int64), (long)524288, null, new System.Configuration.LongValidator(0, 9223372036854775807, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxBufferSize", typeof(System.Int32), 65536, null, new System.Configuration.IntegerValidator(1, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxReceivedMessageSize", typeof(System.Int64), (long)65536, null, new System.Configuration.LongValidator(1, 9223372036854775807, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("proxyAddress", typeof(System.Uri), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("readerQuotas", typeof(System.ServiceModel.Configuration.XmlDictionaryReaderQuotasElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("textEncoding", typeof(System.Text.Encoding), "utf-8", new System.ServiceModel.Configuration.EncodingConverter(), null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("transferMode", typeof(System.ServiceModel.TransferMode), System.ServiceModel.TransferMode.Buffered, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.TransferModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("useDefaultWebProxy", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: HttpDigestClientElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class HttpDigestClientElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("impersonationLevel", typeof(System.Security.Principal.TokenImpersonationLevel), System.Security.Principal.TokenImpersonationLevel.Identification, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.Security.TokenImpersonationLevelHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: HttpMessageHandlerFactoryElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class HttpMessageHandlerFactoryElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("handlers", typeof(System.ServiceModel.Configuration.DelegatingHandlerElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("type", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: HttpsTransportElement

namespace System.ServiceModel.Configuration
{
    public partial class HttpsTransportElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("requireClientCertificate", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: HttpTransportElement

namespace System.ServiceModel.Configuration
{
    public partial class HttpTransportElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("allowCookies", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("requestInitializationTimeout", typeof(System.TimeSpan), System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("authenticationScheme", typeof(System.Net.AuthenticationSchemes), System.Net.AuthenticationSchemes.Anonymous, null, new System.ServiceModel.Configuration.StandardRuntimeFlagEnumValidator<System.Net.AuthenticationSchemes>(), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("bypassProxyOnLocal", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("decompressionEnabled", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("hostNameComparisonMode", typeof(System.ServiceModel.HostNameComparisonMode), System.ServiceModel.HostNameComparisonMode.StrongWildcard, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.HostNameComparisonModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("keepAliveEnabled", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxBufferSize", typeof(System.Int32), 65536, null, new System.Configuration.IntegerValidator(1, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxPendingAccepts", typeof(System.Int32), 0, null, new System.Configuration.IntegerValidator(0, 100000, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("messageHandlerFactory", typeof(System.ServiceModel.Configuration.HttpMessageHandlerFactoryElement), null, null, new System.ServiceModel.Configuration.HttpMessageHandlerFactoryValidator(), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("proxyAddress", typeof(System.Uri), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("proxyAuthenticationScheme", typeof(System.Net.AuthenticationSchemes), System.Net.AuthenticationSchemes.Anonymous, null, new System.ServiceModel.Configuration.StandardRuntimeEnumValidator(typeof(System.Net.AuthenticationSchemes)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("realm", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("transferMode", typeof(System.ServiceModel.TransferMode), System.ServiceModel.TransferMode.Buffered, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.TransferModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("unsafeConnectionNtlmAuthentication", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("useDefaultWebProxy", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("extendedProtectionPolicy", typeof(System.Security.Authentication.ExtendedProtection.Configuration.ExtendedProtectionPolicyElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("webSocketSettings", typeof(System.ServiceModel.Configuration.WebSocketTransportSettingsElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: HttpTransportSecurityElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class HttpTransportSecurityElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("clientCredentialType", typeof(System.ServiceModel.HttpClientCredentialType), System.ServiceModel.HttpClientCredentialType.None, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.HttpClientCredentialTypeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("proxyCredentialType", typeof(System.ServiceModel.HttpProxyCredentialType), System.ServiceModel.HttpProxyCredentialType.None, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.HttpProxyCredentialTypeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("extendedProtectionPolicy", typeof(System.Security.Authentication.ExtendedProtection.Configuration.ExtendedProtectionPolicyElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("realm", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: IdentityElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class IdentityElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("userPrincipalName", typeof(System.ServiceModel.Configuration.UserPrincipalNameElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("servicePrincipalName", typeof(System.ServiceModel.Configuration.ServicePrincipalNameElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("dns", typeof(System.ServiceModel.Configuration.DnsElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("rsa", typeof(System.ServiceModel.Configuration.RsaElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("certificate", typeof(System.ServiceModel.Configuration.CertificateElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("certificateReference", typeof(System.ServiceModel.Configuration.CertificateReferenceElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: IssuedTokenClientElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class IssuedTokenClientElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("localIssuer", typeof(System.ServiceModel.Configuration.IssuedTokenParametersEndpointAddressElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("localIssuerChannelBehaviors", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("issuerChannelBehaviors", typeof(System.ServiceModel.Configuration.IssuedTokenClientBehaviorsElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("cacheIssuedTokens", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxIssuedTokenCachingTime", typeof(System.TimeSpan), System.TimeSpan.Parse("10675199.02:48:05.4775807", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("defaultKeyEntropyMode", typeof(System.ServiceModel.Security.SecurityKeyEntropyMode), System.ServiceModel.Security.SecurityKeyEntropyMode.CombinedEntropy, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.Security.SecurityKeyEntropyModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("issuedTokenRenewalThresholdPercentage", typeof(System.Int32), 60, null, new System.Configuration.IntegerValidator(0, 100, false), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: IssuedTokenServiceElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class IssuedTokenServiceElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("allowedAudienceUris", typeof(System.ServiceModel.Configuration.AllowedAudienceUriElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("audienceUriMode", typeof(System.IdentityModel.Selectors.AudienceUriMode), System.IdentityModel.Selectors.AudienceUriMode.Always, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.IdentityModel.Selectors.AudienceUriModeValidationHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("customCertificateValidatorType", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("certificateValidationMode", typeof(System.ServiceModel.Security.X509CertificateValidationMode), System.ServiceModel.Security.X509CertificateValidationMode.ChainTrust, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.Security.X509CertificateValidationModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("revocationMode", typeof(System.Security.Cryptography.X509Certificates.X509RevocationMode), System.Security.Cryptography.X509Certificates.X509RevocationMode.Online, null, new System.ServiceModel.Configuration.StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.X509RevocationMode)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("trustedStoreLocation", typeof(System.Security.Cryptography.X509Certificates.StoreLocation), System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine, null, new System.ServiceModel.Configuration.StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.StoreLocation)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("samlSerializerType", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("knownCertificates", typeof(System.ServiceModel.Configuration.X509CertificateTrustedIssuerElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("allowUntrustedRsaIssuers", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: IssuedTokenClientBehaviorsElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class IssuedTokenClientBehaviorsElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("issuerAddress", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsRequired | System.Configuration.ConfigurationPropertyOptions.IsKey));
                    properties.Add(new ConfigurationProperty("behaviorConfiguration", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: IssuedTokenParametersElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class IssuedTokenParametersElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("defaultMessageSecurityVersion", typeof(System.ServiceModel.MessageSecurityVersion), null, new System.ServiceModel.Configuration.MessageSecurityVersionConverter(), null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("additionalRequestParameters", typeof(System.ServiceModel.Configuration.XmlElementElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("claimTypeRequirements", typeof(System.ServiceModel.Configuration.ClaimTypeElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("issuer", typeof(System.ServiceModel.Configuration.IssuedTokenParametersEndpointAddressElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("issuerMetadata", typeof(System.ServiceModel.Configuration.EndpointAddressElementBase), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("keySize", typeof(System.Int32), 0, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("keyType", typeof(System.IdentityModel.Tokens.SecurityKeyType), System.IdentityModel.Tokens.SecurityKeyType.SymmetricKey, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.IdentityModel.Tokens.SecurityKeyTypeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("tokenType", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("useStrTransform", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: IssuedTokenParametersEndpointAddressElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class IssuedTokenParametersEndpointAddressElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("binding", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("bindingConfiguration", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: LocalClientSecuritySettingsElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class LocalClientSecuritySettingsElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("cacheCookies", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("detectReplays", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("replayCacheSize", typeof(System.Int32), 900000, null, new System.Configuration.IntegerValidator(1, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxClockSkew", typeof(System.TimeSpan), System.TimeSpan.Parse("00:05:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxCookieCachingTime", typeof(System.TimeSpan), System.TimeSpan.Parse("10675199.02:48:05.4775807", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("replayWindow", typeof(System.TimeSpan), System.TimeSpan.Parse("00:05:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("sessionKeyRenewalInterval", typeof(System.TimeSpan), System.TimeSpan.Parse("10:00:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("sessionKeyRolloverInterval", typeof(System.TimeSpan), System.TimeSpan.Parse("00:05:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("reconnectTransportOnFailure", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("timestampValidityDuration", typeof(System.TimeSpan), System.TimeSpan.Parse("00:05:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("cookieRenewalThresholdPercentage", typeof(System.Int32), 60, null, new System.Configuration.IntegerValidator(0, 100, false), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: LocalServiceSecuritySettingsElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class LocalServiceSecuritySettingsElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("detectReplays", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("issuedCookieLifetime", typeof(System.TimeSpan), System.TimeSpan.Parse("10:00:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxStatefulNegotiations", typeof(System.Int32), 128, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("replayCacheSize", typeof(System.Int32), 900000, null, new System.Configuration.IntegerValidator(1, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxClockSkew", typeof(System.TimeSpan), System.TimeSpan.Parse("00:05:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("negotiationTimeout", typeof(System.TimeSpan), System.TimeSpan.Parse("00:01:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("replayWindow", typeof(System.TimeSpan), System.TimeSpan.Parse("00:05:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("inactivityTimeout", typeof(System.TimeSpan), System.TimeSpan.Parse("00:02:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("sessionKeyRenewalInterval", typeof(System.TimeSpan), System.TimeSpan.Parse("15:00:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("sessionKeyRolloverInterval", typeof(System.TimeSpan), System.TimeSpan.Parse("00:05:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("reconnectTransportOnFailure", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxPendingSessions", typeof(System.Int32), 128, null, new System.Configuration.IntegerValidator(1, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxCachedCookies", typeof(System.Int32), 1000, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("timestampValidityDuration", typeof(System.TimeSpan), System.TimeSpan.Parse("00:05:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: MetadataElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class MetadataElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("policyImporters", typeof(System.ServiceModel.Configuration.PolicyImporterElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("wsdlImporters", typeof(System.ServiceModel.Configuration.WsdlImporterElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: MessageLoggingElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class MessageLoggingElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("logEntireMessage", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("logKnownPii", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("logMalformedMessages", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("logMessagesAtServiceLevel", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("logMessagesAtTransportLevel", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxMessagesToLog", typeof(System.Int32), 10000, null, new System.Configuration.IntegerValidator(-1, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxSizeOfMessageToLog", typeof(System.Int32), 262144, null, new System.Configuration.IntegerValidator(-1, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("filters", typeof(System.ServiceModel.Configuration.XPathMessageFilterElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: MessageSecurityOverHttpElement

namespace System.ServiceModel.Configuration
{
    public partial class MessageSecurityOverHttpElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("clientCredentialType", typeof(System.ServiceModel.MessageCredentialType), System.ServiceModel.MessageCredentialType.Windows, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.MessageCredentialTypeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("negotiateServiceCredential", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("algorithmSuite", typeof(System.ServiceModel.Security.SecurityAlgorithmSuite), "Default", new System.ServiceModel.Configuration.SecurityAlgorithmSuiteConverter(), null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: MessageSecurityOverMsmqElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class MessageSecurityOverMsmqElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("clientCredentialType", typeof(System.ServiceModel.MessageCredentialType), System.ServiceModel.MessageCredentialType.Windows, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.MessageCredentialTypeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("algorithmSuite", typeof(System.ServiceModel.Security.SecurityAlgorithmSuite), "Default", new System.ServiceModel.Configuration.SecurityAlgorithmSuiteConverter(), null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: MessageSecurityOverTcpElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class MessageSecurityOverTcpElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("clientCredentialType", typeof(System.ServiceModel.MessageCredentialType), System.ServiceModel.MessageCredentialType.Windows, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.MessageCredentialTypeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("algorithmSuite", typeof(System.ServiceModel.Security.SecurityAlgorithmSuite), "Default", new System.ServiceModel.Configuration.SecurityAlgorithmSuiteConverter(), null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: MsmqBindingElementBase

namespace System.ServiceModel.Configuration
{
    public partial class MsmqBindingElementBase
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("customDeadLetterQueue", typeof(System.Uri), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("deadLetterQueue", typeof(System.ServiceModel.DeadLetterQueue), System.ServiceModel.DeadLetterQueue.System, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.DeadLetterQueueHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("durable", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("exactlyOnce", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxReceivedMessageSize", typeof(System.Int64), (long)65536, null, new System.Configuration.LongValidator(0, 9223372036854775807, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxRetryCycles", typeof(System.Int32), 2, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("receiveContextEnabled", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("receiveErrorHandling", typeof(System.ServiceModel.ReceiveErrorHandling), System.ServiceModel.ReceiveErrorHandling.Fault, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.ReceiveErrorHandlingHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("receiveRetryCount", typeof(System.Int32), 5, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("retryCycleDelay", typeof(System.TimeSpan), System.TimeSpan.Parse("00:30:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("timeToLive", typeof(System.TimeSpan), System.TimeSpan.Parse("1.00:00:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("useSourceJournal", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("useMsmqTracing", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("validityDuration", typeof(System.TimeSpan), System.TimeSpan.Parse("00:05:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: MsmqIntegrationBindingElement

namespace System.ServiceModel.Configuration
{
    public partial class MsmqIntegrationBindingElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("security", typeof(System.ServiceModel.Configuration.MsmqIntegrationSecurityElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("serializationFormat", typeof(System.ServiceModel.MsmqIntegration.MsmqMessageSerializationFormat), System.ServiceModel.MsmqIntegration.MsmqMessageSerializationFormat.Xml, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.MsmqIntegration.MsmqMessageSerializationFormatHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: MsmqIntegrationElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class MsmqIntegrationElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("serializationFormat", typeof(System.ServiceModel.MsmqIntegration.MsmqMessageSerializationFormat), System.ServiceModel.MsmqIntegration.MsmqMessageSerializationFormat.Xml, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.MsmqIntegration.MsmqMessageSerializationFormatHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: MsmqIntegrationSecurityElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class MsmqIntegrationSecurityElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("mode", typeof(System.ServiceModel.MsmqIntegration.MsmqIntegrationSecurityMode), System.ServiceModel.MsmqIntegration.MsmqIntegrationSecurityMode.Transport, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.MsmqIntegration.MsmqIntegrationSecurityModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("transport", typeof(System.ServiceModel.Configuration.MsmqTransportSecurityElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: MsmqElementBase

namespace System.ServiceModel.Configuration
{
    public partial class MsmqElementBase
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("customDeadLetterQueue", typeof(System.Uri), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("deadLetterQueue", typeof(System.ServiceModel.DeadLetterQueue), System.ServiceModel.DeadLetterQueue.System, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.DeadLetterQueueHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("durable", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("exactlyOnce", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxRetryCycles", typeof(System.Int32), 2, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("receiveContextEnabled", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("receiveErrorHandling", typeof(System.ServiceModel.ReceiveErrorHandling), System.ServiceModel.ReceiveErrorHandling.Fault, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.ReceiveErrorHandlingHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("receiveRetryCount", typeof(System.Int32), 5, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("retryCycleDelay", typeof(System.TimeSpan), System.TimeSpan.Parse("00:30:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("msmqTransportSecurity", typeof(System.ServiceModel.Configuration.MsmqTransportSecurityElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("timeToLive", typeof(System.TimeSpan), System.TimeSpan.Parse("1.00:00:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("useSourceJournal", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("useMsmqTracing", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("validityDuration", typeof(System.TimeSpan), System.TimeSpan.Parse("00:05:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: MsmqTransportElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class MsmqTransportElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("maxPoolSize", typeof(System.Int32), 8, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("queueTransferProtocol", typeof(System.ServiceModel.QueueTransferProtocol), System.ServiceModel.QueueTransferProtocol.Native, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.QueueTransferProtocolHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("useActiveDirectory", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: MsmqTransportSecurityElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class MsmqTransportSecurityElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("msmqAuthenticationMode", typeof(System.ServiceModel.MsmqAuthenticationMode), System.ServiceModel.MsmqAuthenticationMode.WindowsDomain, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.MsmqAuthenticationModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("msmqEncryptionAlgorithm", typeof(System.ServiceModel.MsmqEncryptionAlgorithm), System.ServiceModel.MsmqEncryptionAlgorithm.RC4Stream, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.MsmqEncryptionAlgorithmHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("msmqProtectionLevel", typeof(System.Net.Security.ProtectionLevel), System.Net.Security.ProtectionLevel.Sign, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.Security.ProtectionLevelHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("msmqSecureHashAlgorithm", typeof(System.ServiceModel.MsmqSecureHashAlgorithm), System.ServiceModel.MsmqSecureHashAlgorithm.Sha1, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.MsmqSecureHashAlgorithmHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: MtomMessageEncodingElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class MtomMessageEncodingElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("maxReadPoolSize", typeof(System.Int32), 64, null, new System.Configuration.IntegerValidator(1, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxWritePoolSize", typeof(System.Int32), 16, null, new System.Configuration.IntegerValidator(1, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("messageVersion", typeof(System.ServiceModel.Channels.MessageVersion), "Soap12WSAddressing10", new System.ServiceModel.Configuration.MessageVersionConverter(), null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("readerQuotas", typeof(System.ServiceModel.Configuration.XmlDictionaryReaderQuotasElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxBufferSize", typeof(System.Int32), 65536, null, new System.Configuration.IntegerValidator(1, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("writeEncoding", typeof(System.Text.Encoding), "utf-8", new System.ServiceModel.Configuration.EncodingConverter(), null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: NamedPipeConnectionPoolSettingsElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class NamedPipeConnectionPoolSettingsElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("groupName", typeof(System.String), "default", null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("idleTimeout", typeof(System.TimeSpan), System.TimeSpan.Parse("00:02:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxOutboundConnectionsPerEndpoint", typeof(System.Int32), 10, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: NamedPipeSettingsElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class NamedPipeSettingsElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("applicationContainerSettings", typeof(System.ServiceModel.Configuration.ApplicationContainerSettingsElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: NamedPipeTransportElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class NamedPipeTransportElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("connectionPoolSettings", typeof(System.ServiceModel.Configuration.NamedPipeConnectionPoolSettingsElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("pipeSettings", typeof(System.ServiceModel.Configuration.NamedPipeSettingsElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: NamedPipeTransportSecurityElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class NamedPipeTransportSecurityElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("protectionLevel", typeof(System.Net.Security.ProtectionLevel), System.Net.Security.ProtectionLevel.EncryptAndSign, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.Security.ProtectionLevelHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: NonDualMessageSecurityOverHttpElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class NonDualMessageSecurityOverHttpElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("establishSecurityContext", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: NetHttpBindingElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class NetHttpBindingElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("messageEncoding", typeof(System.ServiceModel.NetHttpMessageEncoding), System.ServiceModel.NetHttpMessageEncoding.Binary, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.NetHttpMessageEncodingHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("reliableSession", typeof(System.ServiceModel.Configuration.StandardBindingOptionalReliableSessionElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("security", typeof(System.ServiceModel.Configuration.BasicHttpSecurityElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("webSocketSettings", typeof(System.ServiceModel.Configuration.NetHttpWebSocketTransportSettingsElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: NetHttpsBindingElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class NetHttpsBindingElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("messageEncoding", typeof(System.ServiceModel.NetHttpMessageEncoding), System.ServiceModel.NetHttpMessageEncoding.Binary, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.NetHttpMessageEncodingHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("reliableSession", typeof(System.ServiceModel.Configuration.StandardBindingOptionalReliableSessionElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("security", typeof(System.ServiceModel.Configuration.BasicHttpsSecurityElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("webSocketSettings", typeof(System.ServiceModel.Configuration.NetHttpWebSocketTransportSettingsElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: NetHttpWebSocketTransportSettingsElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class NetHttpWebSocketTransportSettingsElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Remove("transportUsage");
                    properties.Add(new ConfigurationProperty("transportUsage", typeof(System.ServiceModel.Channels.WebSocketTransportUsage), System.ServiceModel.Channels.WebSocketTransportUsage.WhenDuplex, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.Channels.WebSocketTransportUsageHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Remove("subProtocol");
                    properties.Add(new ConfigurationProperty("subProtocol", typeof(System.String), "soap", null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: WebSocketTransportSettingsElement

namespace System.ServiceModel.Configuration
{
    public partial class WebSocketTransportSettingsElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("transportUsage", typeof(System.ServiceModel.Channels.WebSocketTransportUsage), System.ServiceModel.Channels.WebSocketTransportUsage.Never, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.Channels.WebSocketTransportUsageHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("createNotificationOnConnection", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("keepAliveInterval", typeof(System.TimeSpan), System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("-00:00:00.0010000", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("subProtocol", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("disablePayloadMasking", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxPendingConnections", typeof(System.Int32), 0, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: NetPeerTcpBindingElement

namespace System.ServiceModel.Configuration
{
    public partial class NetPeerTcpBindingElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("listenIPAddress", typeof(System.Net.IPAddress), null, new System.ServiceModel.Configuration.PeerTransportListenAddressConverter(), new System.ServiceModel.Configuration.PeerTransportListenAddressValidator(), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxBufferPoolSize", typeof(System.Int64), (long)524288, null, new System.Configuration.LongValidator(0, 9223372036854775807, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxReceivedMessageSize", typeof(System.Int64), (long)65536, null, new System.Configuration.LongValidator(16384, 9223372036854775807, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("port", typeof(System.Int32), 0, null, new System.Configuration.IntegerValidator(0, 65535, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("readerQuotas", typeof(System.ServiceModel.Configuration.XmlDictionaryReaderQuotasElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("resolver", typeof(System.ServiceModel.Configuration.PeerResolverElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("security", typeof(System.ServiceModel.Configuration.PeerSecurityElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: NetNamedPipeBindingElement

namespace System.ServiceModel.Configuration
{
    public partial class NetNamedPipeBindingElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("transactionFlow", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("transferMode", typeof(System.ServiceModel.TransferMode), System.ServiceModel.TransferMode.Buffered, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.TransferModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("transactionProtocol", typeof(System.ServiceModel.TransactionProtocol), "OleTransactions", new System.ServiceModel.Configuration.TransactionProtocolConverter(), null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("hostNameComparisonMode", typeof(System.ServiceModel.HostNameComparisonMode), System.ServiceModel.HostNameComparisonMode.StrongWildcard, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.HostNameComparisonModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxBufferPoolSize", typeof(System.Int64), (long)524288, null, new System.Configuration.LongValidator(0, 9223372036854775807, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxBufferSize", typeof(System.Int32), 65536, null, new System.Configuration.IntegerValidator(1, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxConnections", typeof(System.Int32), 0, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxReceivedMessageSize", typeof(System.Int64), (long)65536, null, new System.Configuration.LongValidator(1, 9223372036854775807, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("readerQuotas", typeof(System.ServiceModel.Configuration.XmlDictionaryReaderQuotasElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("security", typeof(System.ServiceModel.Configuration.NetNamedPipeSecurityElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: NetNamedPipeSecurityElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class NetNamedPipeSecurityElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("mode", typeof(System.ServiceModel.NetNamedPipeSecurityMode), System.ServiceModel.NetNamedPipeSecurityMode.Transport, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.NetNamedPipeSecurityModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("transport", typeof(System.ServiceModel.Configuration.NamedPipeTransportSecurityElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: NetMsmqBindingElement

namespace System.ServiceModel.Configuration
{
    public partial class NetMsmqBindingElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("queueTransferProtocol", typeof(System.ServiceModel.QueueTransferProtocol), System.ServiceModel.QueueTransferProtocol.Native, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.QueueTransferProtocolHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("readerQuotas", typeof(System.ServiceModel.Configuration.XmlDictionaryReaderQuotasElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxBufferPoolSize", typeof(System.Int64), (long)524288, null, new System.Configuration.LongValidator(0, 9223372036854775807, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("security", typeof(System.ServiceModel.Configuration.NetMsmqSecurityElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("useActiveDirectory", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: NetMsmqSecurityElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class NetMsmqSecurityElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("mode", typeof(System.ServiceModel.NetMsmqSecurityMode), System.ServiceModel.NetMsmqSecurityMode.Transport, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.SecurityModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("transport", typeof(System.ServiceModel.Configuration.MsmqTransportSecurityElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("message", typeof(System.ServiceModel.Configuration.MessageSecurityOverMsmqElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: NetTcpBindingElement

namespace System.ServiceModel.Configuration
{
    public partial class NetTcpBindingElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("transactionFlow", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("transferMode", typeof(System.ServiceModel.TransferMode), System.ServiceModel.TransferMode.Buffered, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.TransferModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("transactionProtocol", typeof(System.ServiceModel.TransactionProtocol), "OleTransactions", new System.ServiceModel.Configuration.TransactionProtocolConverter(), null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("hostNameComparisonMode", typeof(System.ServiceModel.HostNameComparisonMode), System.ServiceModel.HostNameComparisonMode.StrongWildcard, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.HostNameComparisonModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("listenBacklog", typeof(System.Int32), 0, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxBufferPoolSize", typeof(System.Int64), (long)524288, null, new System.Configuration.LongValidator(0, 9223372036854775807, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxBufferSize", typeof(System.Int32), 65536, null, new System.Configuration.IntegerValidator(1, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxConnections", typeof(System.Int32), 0, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxReceivedMessageSize", typeof(System.Int64), (long)65536, null, new System.Configuration.LongValidator(1, 9223372036854775807, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("portSharingEnabled", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("readerQuotas", typeof(System.ServiceModel.Configuration.XmlDictionaryReaderQuotasElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("reliableSession", typeof(System.ServiceModel.Configuration.StandardBindingOptionalReliableSessionElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("security", typeof(System.ServiceModel.Configuration.NetTcpSecurityElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: NetTcpSecurityElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class NetTcpSecurityElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("mode", typeof(System.ServiceModel.SecurityMode), System.ServiceModel.SecurityMode.Transport, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.SecurityModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("transport", typeof(System.ServiceModel.Configuration.TcpTransportSecurityElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("message", typeof(System.ServiceModel.Configuration.MessageSecurityOverTcpElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: StandardBindingOptionalReliableSessionElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class StandardBindingOptionalReliableSessionElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("enabled", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: PeerCredentialElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class PeerCredentialElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("certificate", typeof(System.ServiceModel.Configuration.X509PeerCertificateElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("peerAuthentication", typeof(System.ServiceModel.Configuration.X509PeerCertificateAuthenticationElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("messageSenderAuthentication", typeof(System.ServiceModel.Configuration.X509PeerCertificateAuthenticationElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: PeerCustomResolverElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class PeerCustomResolverElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("address", typeof(System.Uri), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("headers", typeof(System.ServiceModel.Configuration.AddressHeaderCollectionElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("identity", typeof(System.ServiceModel.Configuration.IdentityElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("binding", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("bindingConfiguration", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("resolverType", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: PeerResolverElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class PeerResolverElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("mode", typeof(System.ServiceModel.PeerResolvers.PeerResolverMode), System.ServiceModel.PeerResolvers.PeerResolverMode.Auto, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.PeerResolvers.PeerResolverModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("referralPolicy", typeof(System.ServiceModel.PeerResolvers.PeerReferralPolicy), System.ServiceModel.PeerResolvers.PeerReferralPolicy.Service, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.PeerResolvers.PeerReferralPolicyHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("custom", typeof(System.ServiceModel.Configuration.PeerCustomResolverElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: PeerSecurityElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class PeerSecurityElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("mode", typeof(System.ServiceModel.SecurityMode), System.ServiceModel.SecurityMode.Transport, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.SecurityModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("transport", typeof(System.ServiceModel.Configuration.PeerTransportSecurityElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: PeerTransportElement

namespace System.ServiceModel.Configuration
{
    public partial class PeerTransportElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("listenIPAddress", typeof(System.Net.IPAddress), null, new System.ServiceModel.Configuration.PeerTransportListenAddressConverter(), new System.ServiceModel.Configuration.PeerTransportListenAddressValidator(), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxBufferPoolSize", typeof(System.Int64), (long)524288, null, new System.Configuration.LongValidator(1, 9223372036854775807, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxReceivedMessageSize", typeof(System.Int64), (long)65536, null, new System.Configuration.LongValidator(1, 9223372036854775807, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("port", typeof(System.Int32), 0, null, new System.Configuration.IntegerValidator(0, 65535, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("security", typeof(System.ServiceModel.Configuration.PeerSecurityElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: PeerTransportSecurityElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class PeerTransportSecurityElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("credentialType", typeof(System.ServiceModel.PeerTransportCredentialType), System.ServiceModel.PeerTransportCredentialType.Password, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.PeerTransportCredentialTypeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: PolicyImporterElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class PolicyImporterElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("type", typeof(System.String), null, null, new System.Configuration.StringValidator(1, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsRequired | System.Configuration.ConfigurationPropertyOptions.IsKey));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: PrivacyNoticeElement

namespace System.ServiceModel.Configuration
{
    public partial class PrivacyNoticeElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("url", typeof(System.Uri), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("version", typeof(System.Int32), 0, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: StandardBindingElement

namespace System.ServiceModel.Configuration
{
    public partial class StandardBindingElement
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
                    properties.Add(new ConfigurationProperty("closeTimeout", typeof(System.TimeSpan), System.TimeSpan.Parse("00:01:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("openTimeout", typeof(System.TimeSpan), System.TimeSpan.Parse("00:01:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("receiveTimeout", typeof(System.TimeSpan), System.TimeSpan.Parse("00:10:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("sendTimeout", typeof(System.TimeSpan), System.TimeSpan.Parse("00:01:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: WSHttpBindingBaseElement

namespace System.ServiceModel.Configuration
{
    public partial class WSHttpBindingBaseElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("bypassProxyOnLocal", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("transactionFlow", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("hostNameComparisonMode", typeof(System.ServiceModel.HostNameComparisonMode), System.ServiceModel.HostNameComparisonMode.StrongWildcard, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.HostNameComparisonModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxBufferPoolSize", typeof(System.Int64), (long)524288, null, new System.Configuration.LongValidator(0, 9223372036854775807, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxReceivedMessageSize", typeof(System.Int64), (long)65536, null, new System.Configuration.LongValidator(1, 9223372036854775807, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("messageEncoding", typeof(System.ServiceModel.WSMessageEncoding), System.ServiceModel.WSMessageEncoding.Text, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.WSMessageEncodingHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("proxyAddress", typeof(System.Uri), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("readerQuotas", typeof(System.ServiceModel.Configuration.XmlDictionaryReaderQuotasElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("reliableSession", typeof(System.ServiceModel.Configuration.StandardBindingOptionalReliableSessionElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("textEncoding", typeof(System.Text.Encoding), "utf-8", new System.ServiceModel.Configuration.EncodingConverter(), null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("useDefaultWebProxy", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: WSHttpBindingElement

namespace System.ServiceModel.Configuration
{
    public partial class WSHttpBindingElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("allowCookies", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("security", typeof(System.ServiceModel.Configuration.WSHttpSecurityElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: StandardBindingCollectionElement`2

namespace System.ServiceModel.Configuration
{
    public partial class StandardBindingCollectionElement<TStandardBinding, TBindingConfiguration>
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("", typeof(System.ServiceModel.Configuration.StandardBindingElementCollection<TBindingConfiguration>), null, null, null, System.Configuration.ConfigurationPropertyOptions.IsDefaultCollection));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: TransportElement

namespace System.ServiceModel.Configuration
{
    public partial class TransportElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("manualAddressing", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxBufferPoolSize", typeof(System.Int64), (long)524288, null, new System.Configuration.LongValidator(1, 9223372036854775807, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxReceivedMessageSize", typeof(System.Int64), (long)65536, null, new System.Configuration.LongValidator(1, 9223372036854775807, false), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: StandardEndpointElement

namespace System.ServiceModel.Configuration
{
    public partial class StandardEndpointElement
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
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: StandardEndpointCollectionElement`2

namespace System.ServiceModel.Configuration
{
    public partial class StandardEndpointCollectionElement<TStandardEndpoint, TEndpointConfiguration>
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("", typeof(System.ServiceModel.Configuration.StandardEndpointElementCollection<TEndpointConfiguration>), null, null, null, System.Configuration.ConfigurationPropertyOptions.IsDefaultCollection));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: StandardBindingReliableSessionElement

namespace System.ServiceModel.Configuration
{
    public partial class StandardBindingReliableSessionElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("ordered", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("inactivityTimeout", typeof(System.TimeSpan), System.TimeSpan.Parse("00:10:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00.0000001", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: RsaElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class RsaElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("value", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: ReliableSessionElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class ReliableSessionElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("acknowledgementInterval", typeof(System.TimeSpan), System.TimeSpan.Parse("00:00:00.2", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00.0000001", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("flowControlEnabled", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("inactivityTimeout", typeof(System.TimeSpan), System.TimeSpan.Parse("00:10:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00.0000001", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxPendingChannels", typeof(System.Int32), 4, null, new System.Configuration.IntegerValidator(1, 16384, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxRetryCount", typeof(System.Int32), 8, null, new System.Configuration.IntegerValidator(1, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxTransferWindowSize", typeof(System.Int32), 8, null, new System.Configuration.IntegerValidator(1, 4096, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("ordered", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("reliableMessagingVersion", typeof(System.ServiceModel.ReliableMessagingVersion), "WSReliableMessagingFebruary2005", new System.ServiceModel.Configuration.ReliableMessagingVersionConverter(), null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: SecureConversationServiceElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class SecureConversationServiceElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("securityStateEncoderType", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: SecurityElementBase

namespace System.ServiceModel.Configuration
{
    public partial class SecurityElementBase
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("defaultAlgorithmSuite", typeof(System.ServiceModel.Security.SecurityAlgorithmSuite), "Default", new System.ServiceModel.Configuration.SecurityAlgorithmSuiteConverter(), null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("allowSerializedSigningTokenOnReply", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("enableUnsecuredResponse", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("authenticationMode", typeof(System.ServiceModel.Configuration.AuthenticationMode), System.ServiceModel.Configuration.AuthenticationMode.SspiNegotiated, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.Configuration.AuthenticationModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("requireDerivedKeys", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("securityHeaderLayout", typeof(System.ServiceModel.Channels.SecurityHeaderLayout), System.ServiceModel.Channels.SecurityHeaderLayout.Strict, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.Channels.SecurityHeaderLayoutHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("includeTimestamp", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("allowInsecureTransport", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("keyEntropyMode", typeof(System.ServiceModel.Security.SecurityKeyEntropyMode), System.ServiceModel.Security.SecurityKeyEntropyMode.CombinedEntropy, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.Security.SecurityKeyEntropyModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("issuedTokenParameters", typeof(System.ServiceModel.Configuration.IssuedTokenParametersElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("localClientSettings", typeof(System.ServiceModel.Configuration.LocalClientSecuritySettingsElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("localServiceSettings", typeof(System.ServiceModel.Configuration.LocalServiceSecuritySettingsElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("messageProtectionOrder", typeof(System.ServiceModel.Security.MessageProtectionOrder), System.ServiceModel.Security.MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.Security.MessageProtectionOrderHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("protectTokens", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("messageSecurityVersion", typeof(System.ServiceModel.MessageSecurityVersion), "Default", new System.ServiceModel.Configuration.MessageSecurityVersionConverter(), null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("requireSecurityContextCancellation", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("requireSignatureConfirmation", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("canRenewSecurityContextToken", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: SecurityElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class SecurityElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("secureConversationBootstrap", typeof(System.ServiceModel.Configuration.SecurityElementBase), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: ServiceActivationElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class ServiceActivationElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("relativeAddress", typeof(System.String), null, null, new System.ServiceModel.Configuration.ServiceActivationElement.RelativeAddressValidator(), System.Configuration.ConfigurationPropertyOptions.IsRequired | System.Configuration.ConfigurationPropertyOptions.IsKey));
                    properties.Add(new ConfigurationProperty("service", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("factory", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: ServiceAuthorizationElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class ServiceAuthorizationElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("principalPermissionMode", typeof(System.ServiceModel.Description.PrincipalPermissionMode), System.ServiceModel.Description.PrincipalPermissionMode.UseWindowsGroups, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.Description.PrincipalPermissionModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("roleProviderName", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("impersonateCallerForAllOperations", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("impersonateOnSerializingReply", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("serviceAuthorizationManagerType", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("authorizationPolicies", typeof(System.ServiceModel.Configuration.AuthorizationPolicyTypeElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: ServiceCredentialsElement

namespace System.ServiceModel.Configuration
{
    public partial class ServiceCredentialsElement
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
                    properties.Add(new ConfigurationProperty("clientCertificate", typeof(System.ServiceModel.Configuration.X509InitiatorCertificateServiceElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("serviceCertificate", typeof(System.ServiceModel.Configuration.X509RecipientCertificateServiceElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("userNameAuthentication", typeof(System.ServiceModel.Configuration.UserNameServiceElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("useIdentityConfiguration", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("identityConfiguration", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("windowsAuthentication", typeof(System.ServiceModel.Configuration.WindowsServiceElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("peer", typeof(System.ServiceModel.Configuration.PeerCredentialElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("issuedTokenAuthentication", typeof(System.ServiceModel.Configuration.IssuedTokenServiceElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("secureConversationAuthentication", typeof(System.ServiceModel.Configuration.SecureConversationServiceElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: ServiceElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class ServiceElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("behaviorConfiguration", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("", typeof(System.ServiceModel.Configuration.ServiceEndpointElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.IsDefaultCollection));
                    properties.Add(new ConfigurationProperty("host", typeof(System.ServiceModel.Configuration.HostElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("name", typeof(System.String), null, null, new System.Configuration.StringValidator(1, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsRequired | System.Configuration.ConfigurationPropertyOptions.IsKey));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: ServiceEndpointElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class ServiceEndpointElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("address", typeof(System.Uri), "", null, null, System.Configuration.ConfigurationPropertyOptions.IsKey));
                    properties.Add(new ConfigurationProperty("behaviorConfiguration", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("binding", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsKey));
                    properties.Add(new ConfigurationProperty("bindingConfiguration", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsKey));
                    properties.Add(new ConfigurationProperty("name", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("bindingName", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsKey));
                    properties.Add(new ConfigurationProperty("bindingNamespace", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsKey));
                    properties.Add(new ConfigurationProperty("contract", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsKey));
                    properties.Add(new ConfigurationProperty("headers", typeof(System.ServiceModel.Configuration.AddressHeaderCollectionElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("identity", typeof(System.ServiceModel.Configuration.IdentityElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("listenUriMode", typeof(System.ServiceModel.Description.ListenUriMode), System.ServiceModel.Description.ListenUriMode.Explicit, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.Description.ListenUriModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("listenUri", typeof(System.Uri), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("isSystemEndpoint", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("kind", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsKey));
                    properties.Add(new ConfigurationProperty("endpointConfiguration", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsKey));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: ExtensionElement

namespace System.ServiceModel.Configuration
{
    public partial class ExtensionElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("name", typeof(System.String), null, null, new System.Configuration.StringValidator(1, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsRequired | System.Configuration.ConfigurationPropertyOptions.IsKey));
                    properties.Add(new ConfigurationProperty("type", typeof(System.String), null, null, new System.Configuration.StringValidator(1, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsRequired));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: ServiceHostingEnvironmentSection

namespace System.ServiceModel.Configuration
{
    public sealed partial class ServiceHostingEnvironmentSection
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("", typeof(System.ServiceModel.Configuration.TransportConfigurationTypeElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.IsDefaultCollection));
                    properties.Add(new ConfigurationProperty("baseAddressPrefixFilters", typeof(System.ServiceModel.Configuration.BaseAddressPrefixFilterElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("serviceActivations", typeof(System.ServiceModel.Configuration.ServiceActivationElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("aspNetCompatibilityEnabled", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("closeIdleServicesAtLowMemory", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("minFreeMemoryPercentageToActivateService", typeof(System.Int32), 5, null, new System.Configuration.IntegerValidator(0, 99, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("multipleSiteBindingsEnabled", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: ProtocolMappingSection

namespace System.ServiceModel.Configuration
{
    public sealed partial class ProtocolMappingSection
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("", typeof(System.ServiceModel.Configuration.ProtocolMappingElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.IsDefaultCollection));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: ProtocolMappingElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class ProtocolMappingElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("scheme", typeof(System.String), null, null, new System.Configuration.StringValidator(1, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsRequired | System.Configuration.ConfigurationPropertyOptions.IsKey));
                    properties.Add(new ConfigurationProperty("binding", typeof(System.String), null, null, new System.Configuration.StringValidator(1, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsRequired));
                    properties.Add(new ConfigurationProperty("bindingConfiguration", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: ServiceMetadataPublishingElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class ServiceMetadataPublishingElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("externalMetadataLocation", typeof(System.Uri), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("httpGetEnabled", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("httpGetUrl", typeof(System.Uri), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("httpsGetEnabled", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("httpsGetUrl", typeof(System.Uri), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("httpGetBinding", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("httpGetBindingConfiguration", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("httpsGetBinding", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("httpsGetBindingConfiguration", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("policyVersion", typeof(System.ServiceModel.Description.PolicyVersion), "Default", new System.ServiceModel.Configuration.PolicyVersionConverter(), null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: ExtensionsSection

namespace System.ServiceModel.Configuration
{
    public partial class ExtensionsSection
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("behaviorExtensions", typeof(System.ServiceModel.Configuration.ExtensionElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("bindingElementExtensions", typeof(System.ServiceModel.Configuration.ExtensionElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("bindingExtensions", typeof(System.ServiceModel.Configuration.ExtensionElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("endpointExtensions", typeof(System.ServiceModel.Configuration.ExtensionElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: ServiceSecurityAuditElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class ServiceSecurityAuditElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("auditLogLocation", typeof(System.ServiceModel.AuditLogLocation), System.ServiceModel.AuditLogLocation.Default, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.AuditLogLocationHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("suppressAuditFailure", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("serviceAuthorizationAuditLevel", typeof(System.ServiceModel.AuditLevel), System.ServiceModel.AuditLevel.None, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.AuditLevelHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("messageAuthenticationAuditLevel", typeof(System.ServiceModel.AuditLevel), System.ServiceModel.AuditLevel.None, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.AuditLevelHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: ServicesSection

namespace System.ServiceModel.Configuration
{
    public sealed partial class ServicesSection
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("", typeof(System.ServiceModel.Configuration.ServiceElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.IsDefaultCollection));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: ServiceThrottlingElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class ServiceThrottlingElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("maxConcurrentCalls", typeof(System.Int32), 16, null, new System.Configuration.IntegerValidator(1, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxConcurrentSessions", typeof(System.Int32), 100, null, new System.Configuration.IntegerValidator(1, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxConcurrentInstances", typeof(System.Int32), 116, null, new System.Configuration.IntegerValidator(1, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: ServicePrincipalNameElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class ServicePrincipalNameElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("value", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: SslStreamSecurityElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class SslStreamSecurityElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("requireClientCertificate", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("sslProtocols", typeof(System.Security.Authentication.SslProtocols), System.Security.Authentication.SslProtocols.Tls | System.Security.Authentication.SslProtocols.Tls11 | System.Security.Authentication.SslProtocols.Tls12, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.Security.SslProtocolsHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: TcpTransportElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class TcpTransportElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("listenBacklog", typeof(System.Int32), 0, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("portSharingEnabled", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("teredoEnabled", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("connectionPoolSettings", typeof(System.ServiceModel.Configuration.TcpConnectionPoolSettingsElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("extendedProtectionPolicy", typeof(System.Security.Authentication.ExtendedProtection.Configuration.ExtendedProtectionPolicyElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: TcpTransportSecurityElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class TcpTransportSecurityElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("clientCredentialType", typeof(System.ServiceModel.TcpClientCredentialType), System.ServiceModel.TcpClientCredentialType.Windows, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.TcpClientCredentialTypeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("protectionLevel", typeof(System.Net.Security.ProtectionLevel), System.Net.Security.ProtectionLevel.EncryptAndSign, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.Security.ProtectionLevelHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("extendedProtectionPolicy", typeof(System.Security.Authentication.ExtendedProtection.Configuration.ExtendedProtectionPolicyElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("sslProtocols", typeof(System.Security.Authentication.SslProtocols), System.Security.Authentication.SslProtocols.Tls | System.Security.Authentication.SslProtocols.Tls11 | System.Security.Authentication.SslProtocols.Tls12, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.Security.SslProtocolsHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: TextMessageEncodingElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class TextMessageEncodingElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("maxReadPoolSize", typeof(System.Int32), 64, null, new System.Configuration.IntegerValidator(1, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxWritePoolSize", typeof(System.Int32), 16, null, new System.Configuration.IntegerValidator(1, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("messageVersion", typeof(System.ServiceModel.Channels.MessageVersion), "Soap12WSAddressing10", new System.ServiceModel.Configuration.MessageVersionConverter(), null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("readerQuotas", typeof(System.ServiceModel.Configuration.XmlDictionaryReaderQuotasElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("writeEncoding", typeof(System.Text.Encoding), "utf-8", new System.ServiceModel.Configuration.EncodingConverter(), null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: TransactedBatchingElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class TransactedBatchingElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("maxBatchSize", typeof(System.Int32), 0, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: TransportConfigurationTypeElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class TransportConfigurationTypeElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("name", typeof(System.String), null, null, new System.Configuration.StringValidator(1, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsRequired | System.Configuration.ConfigurationPropertyOptions.IsKey));
                    properties.Add(new ConfigurationProperty("transportConfigurationType", typeof(System.String), null, null, new System.Configuration.StringValidator(1, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsRequired));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: UserPrincipalNameElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class UserPrincipalNameElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("value", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: UserNameServiceElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class UserNameServiceElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("userNamePasswordValidationMode", typeof(System.ServiceModel.Security.UserNamePasswordValidationMode), System.ServiceModel.Security.UserNamePasswordValidationMode.Windows, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.Security.UserNamePasswordValidationModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("includeWindowsGroups", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("membershipProviderName", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("customUserNamePasswordValidatorType", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("cacheLogonTokens", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxCachedLogonTokens", typeof(System.Int32), 128, null, new System.Configuration.IntegerValidator(1, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("cachedLogonTokenLifetime", typeof(System.TimeSpan), System.TimeSpan.Parse("00:15:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00.0000001", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: WindowsClientElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class WindowsClientElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("allowNtlm", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("allowedImpersonationLevel", typeof(System.Security.Principal.TokenImpersonationLevel), System.Security.Principal.TokenImpersonationLevel.Identification, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.Security.TokenImpersonationLevelHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: WindowsServiceElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class WindowsServiceElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("includeWindowsGroups", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("allowAnonymousLogons", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: WindowsStreamSecurityElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class WindowsStreamSecurityElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("protectionLevel", typeof(System.Net.Security.ProtectionLevel), System.Net.Security.ProtectionLevel.EncryptAndSign, null, new System.ServiceModel.Configuration.StandardRuntimeEnumValidator(typeof(System.Net.Security.ProtectionLevel)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: WsdlImporterElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class WsdlImporterElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("type", typeof(System.String), null, null, new System.Configuration.StringValidator(1, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsRequired | System.Configuration.ConfigurationPropertyOptions.IsKey));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: WSDualHttpBindingElement

namespace System.ServiceModel.Configuration
{
    public partial class WSDualHttpBindingElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("bypassProxyOnLocal", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("clientBaseAddress", typeof(System.Uri), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("transactionFlow", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("hostNameComparisonMode", typeof(System.ServiceModel.HostNameComparisonMode), System.ServiceModel.HostNameComparisonMode.StrongWildcard, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.HostNameComparisonModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxBufferPoolSize", typeof(System.Int64), (long)524288, null, new System.Configuration.LongValidator(0, 9223372036854775807, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxReceivedMessageSize", typeof(System.Int64), (long)65536, null, new System.Configuration.LongValidator(1, 9223372036854775807, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("messageEncoding", typeof(System.ServiceModel.WSMessageEncoding), System.ServiceModel.WSMessageEncoding.Text, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.WSMessageEncodingHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("proxyAddress", typeof(System.Uri), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("readerQuotas", typeof(System.ServiceModel.Configuration.XmlDictionaryReaderQuotasElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("reliableSession", typeof(System.ServiceModel.Configuration.StandardBindingReliableSessionElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("security", typeof(System.ServiceModel.Configuration.WSDualHttpSecurityElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("textEncoding", typeof(System.Text.Encoding), "utf-8", new System.ServiceModel.Configuration.EncodingConverter(), null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("useDefaultWebProxy", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: WSDualHttpSecurityElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class WSDualHttpSecurityElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("mode", typeof(System.ServiceModel.WSDualHttpSecurityMode), System.ServiceModel.WSDualHttpSecurityMode.Message, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.WSDualHttpSecurityModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("message", typeof(System.ServiceModel.Configuration.MessageSecurityOverHttpElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: WSHttpSecurityElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class WSHttpSecurityElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("mode", typeof(System.ServiceModel.SecurityMode), System.ServiceModel.SecurityMode.Message, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.SecurityModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("transport", typeof(System.ServiceModel.Configuration.WSHttpTransportSecurityElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("message", typeof(System.ServiceModel.Configuration.NonDualMessageSecurityOverHttpElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: XmlDictionaryReaderQuotasElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class XmlDictionaryReaderQuotasElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("maxDepth", typeof(System.Int32), 0, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxStringContentLength", typeof(System.Int32), 0, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxArrayLength", typeof(System.Int32), 0, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxBytesPerRead", typeof(System.Int32), 0, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxNameTableCharCount", typeof(System.Int32), 0, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: XmlElementElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class XmlElementElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("xmlElement", typeof(System.Xml.XmlElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.IsKey));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: DataContractSerializerElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class DataContractSerializerElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("ignoreExtensionDataObject", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxItemsInObjectGraph", typeof(System.Int32), 2147483647, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: XPathMessageFilterElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class XPathMessageFilterElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("filter", typeof(System.ServiceModel.Dispatcher.XPathMessageFilter), null, null, null, System.Configuration.ConfigurationPropertyOptions.IsRequired | System.Configuration.ConfigurationPropertyOptions.IsKey));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: X509CertificateTrustedIssuerElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class X509CertificateTrustedIssuerElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("findValue", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsKey));
                    properties.Add(new ConfigurationProperty("storeLocation", typeof(System.Security.Cryptography.X509Certificates.StoreLocation), System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine, null, new System.ServiceModel.Configuration.StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.StoreLocation)), System.Configuration.ConfigurationPropertyOptions.IsKey));
                    properties.Add(new ConfigurationProperty("storeName", typeof(System.Security.Cryptography.X509Certificates.StoreName), System.Security.Cryptography.X509Certificates.StoreName.My, null, new System.ServiceModel.Configuration.StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.StoreName)), System.Configuration.ConfigurationPropertyOptions.IsKey));
                    properties.Add(new ConfigurationProperty("x509FindType", typeof(System.Security.Cryptography.X509Certificates.X509FindType), System.Security.Cryptography.X509Certificates.X509FindType.FindBySubjectDistinguishedName, null, new System.ServiceModel.Configuration.StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.X509FindType)), System.Configuration.ConfigurationPropertyOptions.IsKey));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: X509ClientCertificateAuthenticationElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class X509ClientCertificateAuthenticationElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("customCertificateValidatorType", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("certificateValidationMode", typeof(System.ServiceModel.Security.X509CertificateValidationMode), System.ServiceModel.Security.X509CertificateValidationMode.ChainTrust, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.Security.X509CertificateValidationModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("revocationMode", typeof(System.Security.Cryptography.X509Certificates.X509RevocationMode), System.Security.Cryptography.X509Certificates.X509RevocationMode.Online, null, new System.ServiceModel.Configuration.StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.X509RevocationMode)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("trustedStoreLocation", typeof(System.Security.Cryptography.X509Certificates.StoreLocation), System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine, null, new System.ServiceModel.Configuration.StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.StoreLocation)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("includeWindowsGroups", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("mapClientCertificateToWindowsAccount", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: X509ClientCertificateCredentialsElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class X509ClientCertificateCredentialsElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("findValue", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("storeLocation", typeof(System.Security.Cryptography.X509Certificates.StoreLocation), System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine, null, new System.ServiceModel.Configuration.StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.StoreLocation)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("storeName", typeof(System.Security.Cryptography.X509Certificates.StoreName), System.Security.Cryptography.X509Certificates.StoreName.My, null, new System.ServiceModel.Configuration.StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.StoreName)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("x509FindType", typeof(System.Security.Cryptography.X509Certificates.X509FindType), System.Security.Cryptography.X509Certificates.X509FindType.FindBySubjectDistinguishedName, null, new System.ServiceModel.Configuration.StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.X509FindType)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: X509DefaultServiceCertificateElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class X509DefaultServiceCertificateElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("findValue", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("storeLocation", typeof(System.Security.Cryptography.X509Certificates.StoreLocation), System.Security.Cryptography.X509Certificates.StoreLocation.CurrentUser, null, new System.ServiceModel.Configuration.StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.StoreLocation)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("storeName", typeof(System.Security.Cryptography.X509Certificates.StoreName), System.Security.Cryptography.X509Certificates.StoreName.My, null, new System.ServiceModel.Configuration.StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.StoreName)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("x509FindType", typeof(System.Security.Cryptography.X509Certificates.X509FindType), System.Security.Cryptography.X509Certificates.X509FindType.FindBySubjectDistinguishedName, null, new System.ServiceModel.Configuration.StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.X509FindType)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: X509InitiatorCertificateClientElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class X509InitiatorCertificateClientElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("findValue", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("storeLocation", typeof(System.Security.Cryptography.X509Certificates.StoreLocation), System.Security.Cryptography.X509Certificates.StoreLocation.CurrentUser, null, new System.ServiceModel.Configuration.StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.StoreLocation)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("storeName", typeof(System.Security.Cryptography.X509Certificates.StoreName), System.Security.Cryptography.X509Certificates.StoreName.My, null, new System.ServiceModel.Configuration.StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.StoreName)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("x509FindType", typeof(System.Security.Cryptography.X509Certificates.X509FindType), System.Security.Cryptography.X509Certificates.X509FindType.FindBySubjectDistinguishedName, null, new System.ServiceModel.Configuration.StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.X509FindType)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: X509RecipientCertificateClientElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class X509RecipientCertificateClientElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("defaultCertificate", typeof(System.ServiceModel.Configuration.X509DefaultServiceCertificateElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("scopedCertificates", typeof(System.ServiceModel.Configuration.X509ScopedServiceCertificateElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("authentication", typeof(System.ServiceModel.Configuration.X509ServiceCertificateAuthenticationElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("sslCertificateAuthentication", typeof(System.ServiceModel.Configuration.X509ServiceCertificateAuthenticationElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: X509InitiatorCertificateServiceElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class X509InitiatorCertificateServiceElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("certificate", typeof(System.ServiceModel.Configuration.X509ClientCertificateCredentialsElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("authentication", typeof(System.ServiceModel.Configuration.X509ClientCertificateAuthenticationElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: X509RecipientCertificateServiceElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class X509RecipientCertificateServiceElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("findValue", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("storeLocation", typeof(System.Security.Cryptography.X509Certificates.StoreLocation), System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine, null, new System.ServiceModel.Configuration.StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.StoreLocation)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("storeName", typeof(System.Security.Cryptography.X509Certificates.StoreName), System.Security.Cryptography.X509Certificates.StoreName.My, null, new System.ServiceModel.Configuration.StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.StoreName)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("x509FindType", typeof(System.Security.Cryptography.X509Certificates.X509FindType), System.Security.Cryptography.X509Certificates.X509FindType.FindBySubjectDistinguishedName, null, new System.ServiceModel.Configuration.StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.X509FindType)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: X509ScopedServiceCertificateElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class X509ScopedServiceCertificateElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("targetUri", typeof(System.Uri), null, null, null, System.Configuration.ConfigurationPropertyOptions.IsRequired | System.Configuration.ConfigurationPropertyOptions.IsKey));
                    properties.Add(new ConfigurationProperty("findValue", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("storeLocation", typeof(System.Security.Cryptography.X509Certificates.StoreLocation), System.Security.Cryptography.X509Certificates.StoreLocation.CurrentUser, null, new System.ServiceModel.Configuration.StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.StoreLocation)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("storeName", typeof(System.Security.Cryptography.X509Certificates.StoreName), System.Security.Cryptography.X509Certificates.StoreName.My, null, new System.ServiceModel.Configuration.StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.StoreName)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("x509FindType", typeof(System.Security.Cryptography.X509Certificates.X509FindType), System.Security.Cryptography.X509Certificates.X509FindType.FindBySubjectDistinguishedName, null, new System.ServiceModel.Configuration.StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.X509FindType)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: X509ServiceCertificateAuthenticationElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class X509ServiceCertificateAuthenticationElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("customCertificateValidatorType", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("certificateValidationMode", typeof(System.ServiceModel.Security.X509CertificateValidationMode), System.ServiceModel.Security.X509CertificateValidationMode.ChainTrust, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.Security.X509CertificateValidationModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("revocationMode", typeof(System.Security.Cryptography.X509Certificates.X509RevocationMode), System.Security.Cryptography.X509Certificates.X509RevocationMode.Online, null, new System.ServiceModel.Configuration.StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.X509RevocationMode)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("trustedStoreLocation", typeof(System.Security.Cryptography.X509Certificates.StoreLocation), System.Security.Cryptography.X509Certificates.StoreLocation.CurrentUser, null, new System.ServiceModel.Configuration.StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.StoreLocation)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: ComContractElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class ComContractElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("contract", typeof(System.String), null, null, new System.Configuration.StringValidator(1, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsRequired | System.Configuration.ConfigurationPropertyOptions.IsKey));
                    properties.Add(new ConfigurationProperty("exposedMethods", typeof(System.ServiceModel.Configuration.ComMethodElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("name", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("namespace", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("persistableTypes", typeof(System.ServiceModel.Configuration.ComPersistableTypeElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("requiresSession", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("userDefinedTypes", typeof(System.ServiceModel.Configuration.ComUdtElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: ComContractsSection

namespace System.ServiceModel.Configuration
{
    public sealed partial class ComContractsSection
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("", typeof(System.ServiceModel.Configuration.ComContractElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.IsDefaultCollection));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: ComMethodElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class ComMethodElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("exposedMethod", typeof(System.String), null, null, new System.Configuration.StringValidator(1, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsRequired | System.Configuration.ConfigurationPropertyOptions.IsKey));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: ComPersistableTypeElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class ComPersistableTypeElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("name", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("ID", typeof(System.String), null, null, new System.Configuration.StringValidator(1, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsRequired | System.Configuration.ConfigurationPropertyOptions.IsKey));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: ComUdtElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class ComUdtElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("name", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("typeLibID", typeof(System.String), null, null, new System.Configuration.StringValidator(1, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsRequired));
                    properties.Add(new ConfigurationProperty("typeLibVersion", typeof(System.String), null, null, new System.Configuration.StringValidator(1, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsRequired));
                    properties.Add(new ConfigurationProperty("typeDefID", typeof(System.String), null, null, new System.Configuration.StringValidator(1, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsRequired | System.Configuration.ConfigurationPropertyOptions.IsKey));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: TransactionFlowElement

namespace System.ServiceModel.Configuration
{
    public partial class TransactionFlowElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("transactionProtocol", typeof(System.ServiceModel.TransactionProtocol), "OleTransactions", new System.ServiceModel.Configuration.TransactionProtocolConverter(), null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("allowWildcardAction", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: WSFederationHttpBindingElement

namespace System.ServiceModel.Configuration
{
    public partial class WSFederationHttpBindingElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("privacyNoticeAt", typeof(System.Uri), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("privacyNoticeVersion", typeof(System.Int32), 0, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("security", typeof(System.ServiceModel.Configuration.WSFederationHttpSecurityElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: WSFederationHttpSecurityElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class WSFederationHttpSecurityElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("mode", typeof(System.ServiceModel.WSFederationHttpSecurityMode), System.ServiceModel.WSFederationHttpSecurityMode.Message, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.WSFederationHttpSecurityModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("message", typeof(System.ServiceModel.Configuration.FederatedMessageSecurityOverHttpElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: X509PeerCertificateElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class X509PeerCertificateElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("findValue", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("storeLocation", typeof(System.Security.Cryptography.X509Certificates.StoreLocation), System.Security.Cryptography.X509Certificates.StoreLocation.CurrentUser, null, new System.ServiceModel.Configuration.StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.StoreLocation)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("storeName", typeof(System.Security.Cryptography.X509Certificates.StoreName), System.Security.Cryptography.X509Certificates.StoreName.My, null, new System.ServiceModel.Configuration.StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.StoreName)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("x509FindType", typeof(System.Security.Cryptography.X509Certificates.X509FindType), System.Security.Cryptography.X509Certificates.X509FindType.FindBySubjectDistinguishedName, null, new System.ServiceModel.Configuration.StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.X509FindType)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: X509PeerCertificateAuthenticationElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class X509PeerCertificateAuthenticationElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("customCertificateValidatorType", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("certificateValidationMode", typeof(System.ServiceModel.Security.X509CertificateValidationMode), System.ServiceModel.Security.X509CertificateValidationMode.PeerOrChainTrust, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.Security.X509CertificateValidationModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("revocationMode", typeof(System.Security.Cryptography.X509Certificates.X509RevocationMode), System.Security.Cryptography.X509Certificates.X509RevocationMode.Online, null, new System.ServiceModel.Configuration.StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.X509RevocationMode)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("trustedStoreLocation", typeof(System.Security.Cryptography.X509Certificates.StoreLocation), System.Security.Cryptography.X509Certificates.StoreLocation.CurrentUser, null, new System.ServiceModel.Configuration.StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.StoreLocation)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: ServiceDebugElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class ServiceDebugElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("httpHelpPageEnabled", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("httpHelpPageUrl", typeof(System.Uri), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("httpsHelpPageEnabled", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("httpsHelpPageUrl", typeof(System.Uri), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("httpHelpPageBinding", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("httpHelpPageBindingConfiguration", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("httpsHelpPageBinding", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("httpsHelpPageBindingConfiguration", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("includeExceptionDetailInFaults", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: ServiceTimeoutsElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class ServiceTimeoutsElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("transactionTimeout", typeof(System.TimeSpan), System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: TcpConnectionPoolSettingsElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class TcpConnectionPoolSettingsElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("groupName", typeof(System.String), "default", null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("leaseTimeout", typeof(System.TimeSpan), System.TimeSpan.Parse("00:05:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("idleTimeout", typeof(System.TimeSpan), System.TimeSpan.Parse("00:02:00", CultureInfo.InvariantCulture), new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxOutboundConnectionsPerEndpoint", typeof(System.Int32), 10, null, new System.Configuration.IntegerValidator(0, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: WSHttpTransportSecurityElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class WSHttpTransportSecurityElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("clientCredentialType", typeof(System.ServiceModel.HttpClientCredentialType), System.ServiceModel.HttpClientCredentialType.Windows, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.HttpClientCredentialTypeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("proxyCredentialType", typeof(System.ServiceModel.HttpProxyCredentialType), System.ServiceModel.HttpProxyCredentialType.None, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.HttpProxyCredentialTypeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("extendedProtectionPolicy", typeof(System.Security.Authentication.ExtendedProtection.Configuration.ExtendedProtectionPolicyElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("realm", typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: DispatcherSynchronizationElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class DispatcherSynchronizationElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("asynchronousSendEnabled", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxPendingReceives", typeof(System.Int32), 1, null, new System.Configuration.IntegerValidator(1, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: RemoveBehaviorElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class RemoveBehaviorElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("name", typeof(System.String), null, null, new System.Configuration.StringValidator(1, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsRequired));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

// configType.Name: UseRequestHeadersForMetadataAddressElement

namespace System.ServiceModel.Configuration
{
    public sealed partial class UseRequestHeadersForMetadataAddressElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty("defaultPorts", typeof(System.ServiceModel.Configuration.DefaultPortElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

