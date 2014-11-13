//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Globalization;
    using System.Net;
    using System.Net.Security;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.IdentityModel.Tokens;
    using System.ComponentModel;
    using System.Xml;

    public sealed partial class FederatedMessageSecurityOverHttpElement : ServiceModelConfigurationElement
    {

        [ConfigurationProperty(ConfigurationStrings.AlgorithmSuite, DefaultValue = ConfigurationStrings.Default)]
        [TypeConverter(typeof(SecurityAlgorithmSuiteConverter))]
        public SecurityAlgorithmSuite AlgorithmSuite
        {
            get { return (SecurityAlgorithmSuite)base[ConfigurationStrings.AlgorithmSuite]; }
            set { base[ConfigurationStrings.AlgorithmSuite] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ClaimTypeRequirements)]
        public ClaimTypeElementCollection ClaimTypeRequirements
        {
            get { return (ClaimTypeElementCollection)base[ConfigurationStrings.ClaimTypeRequirements]; }
        }

        [ConfigurationProperty(ConfigurationStrings.EstablishSecurityContext, DefaultValue = FederatedMessageSecurityOverHttp.DefaultEstablishSecurityContext)]
        public bool EstablishSecurityContext
        {
            get { return (bool)base[ConfigurationStrings.EstablishSecurityContext]; }
            set { base[ConfigurationStrings.EstablishSecurityContext] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.IssuedKeyType, DefaultValue = FederatedMessageSecurityOverHttp.DefaultIssuedKeyType)]
        [ServiceModelEnumValidator(typeof(System.IdentityModel.Tokens.SecurityKeyTypeHelper))]
        public SecurityKeyType IssuedKeyType
        {
            get { return (SecurityKeyType)base[ConfigurationStrings.IssuedKeyType]; }
            set { base[ConfigurationStrings.IssuedKeyType] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.IssuedTokenType, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string IssuedTokenType
        {
            get { return (string)base[ConfigurationStrings.IssuedTokenType]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }

                base[ConfigurationStrings.IssuedTokenType] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.Issuer)]
        public IssuedTokenParametersEndpointAddressElement Issuer
        {
            get { return (IssuedTokenParametersEndpointAddressElement)base[ConfigurationStrings.Issuer]; }
        }

        [ConfigurationProperty(ConfigurationStrings.IssuerMetadata)]
        public EndpointAddressElementBase IssuerMetadata
        {
            get { return (EndpointAddressElementBase)base[ConfigurationStrings.IssuerMetadata]; }
        }

        [ConfigurationProperty(ConfigurationStrings.NegotiateServiceCredential, DefaultValue = MessageSecurityOverHttp.DefaultNegotiateServiceCredential)]
        public bool NegotiateServiceCredential
        {
            get { return (bool)base[ConfigurationStrings.NegotiateServiceCredential]; }
            set { base[ConfigurationStrings.NegotiateServiceCredential] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.TokenRequestParameters)]
        public XmlElementElementCollection TokenRequestParameters
        {
            get { return (XmlElementElementCollection)base[ConfigurationStrings.TokenRequestParameters]; }
        }

        internal void ApplyConfiguration(FederatedMessageSecurityOverHttp security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            security.NegotiateServiceCredential = this.NegotiateServiceCredential;
            security.AlgorithmSuite = this.AlgorithmSuite;
            security.IssuedKeyType = this.IssuedKeyType;
            security.EstablishSecurityContext = this.EstablishSecurityContext;
            if (!string.IsNullOrEmpty(this.IssuedTokenType))
            {
                security.IssuedTokenType = this.IssuedTokenType;
            }
            if (PropertyValueOrigin.Default != this.ElementInformation.Properties[ConfigurationStrings.Issuer].ValueOrigin)
            {
                security.IssuerAddress = ConfigLoader.LoadEndpointAddress(this.Issuer);

                if (!string.IsNullOrEmpty(this.Issuer.Binding))
                {
                    security.IssuerBinding = ConfigLoader.LookupBinding(this.Issuer.Binding, this.Issuer.BindingConfiguration, this.EvaluationContext);
                }
            }
            if (PropertyValueOrigin.Default != this.ElementInformation.Properties[ConfigurationStrings.IssuerMetadata].ValueOrigin)
            {
                security.IssuerMetadataAddress = ConfigLoader.LoadEndpointAddress(this.IssuerMetadata);
            }
            foreach (XmlElementElement xmlElement in this.TokenRequestParameters)
            {
                security.TokenRequestParameters.Add(xmlElement.XmlElement);
            }
            foreach (ClaimTypeElement claimType in this.ClaimTypeRequirements)
            {
                security.ClaimTypeRequirements.Add(new ClaimTypeRequirement(claimType.ClaimType, claimType.IsOptional));
            }
        }

        internal void InitializeFrom(FederatedMessageSecurityOverHttp security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.NegotiateServiceCredential, security.NegotiateServiceCredential);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.AlgorithmSuite, security.AlgorithmSuite);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.IssuedKeyType, security.IssuedKeyType);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.EstablishSecurityContext, security.EstablishSecurityContext);
            if (security.IssuedTokenType != null)
            {
                this.IssuedTokenType = security.IssuedTokenType;
            }
            if (security.IssuerAddress != null)
            {
                this.Issuer.InitializeFrom(security.IssuerAddress);
            }
            if (security.IssuerMetadataAddress != null)
            {
                this.IssuerMetadata.InitializeFrom(security.IssuerMetadataAddress);
            }
            string bindingType = null;
            if (security.IssuerBinding != null)
            {
                if (null == this.Issuer.Address)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigNullIssuerAddress)));
                }

                this.Issuer.BindingConfiguration = this.Issuer.Address.ToString();
                BindingsSection.TryAdd(this.Issuer.BindingConfiguration,
                    security.IssuerBinding, out bindingType);
                this.Issuer.Binding = bindingType;
            }
            foreach (XmlElement element in security.TokenRequestParameters)
            {
                this.TokenRequestParameters.Add(new XmlElementElement(element));
            }
            foreach (ClaimTypeRequirement claimTypeRequirement in security.ClaimTypeRequirements)
            {
                ClaimTypeElement element = new ClaimTypeElement(claimTypeRequirement.ClaimType, claimTypeRequirement.IsOptional);
                this.ClaimTypeRequirements.Add(element);
            }
        }
    }
}
