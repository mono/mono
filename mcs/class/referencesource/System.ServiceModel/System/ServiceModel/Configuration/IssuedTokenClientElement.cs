//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Configuration;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;

    public sealed partial class IssuedTokenClientElement : ConfigurationElement
    {
        public IssuedTokenClientElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.LocalIssuer)]
        public IssuedTokenParametersEndpointAddressElement LocalIssuer
        {
            get { return (IssuedTokenParametersEndpointAddressElement)base[ConfigurationStrings.LocalIssuer]; }
        }

        [ConfigurationProperty(ConfigurationStrings.LocalIssuerChannelBehaviors, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string LocalIssuerChannelBehaviors
        {
            get { return (string)base[ConfigurationStrings.LocalIssuerChannelBehaviors]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }
                base[ConfigurationStrings.LocalIssuerChannelBehaviors] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.IssuerChannelBehaviors)]
        public IssuedTokenClientBehaviorsElementCollection IssuerChannelBehaviors
        {
            get { return (IssuedTokenClientBehaviorsElementCollection)base[ConfigurationStrings.IssuerChannelBehaviors]; }
        }

        [ConfigurationProperty(ConfigurationStrings.CacheIssuedTokens, DefaultValue = SpnegoTokenProvider.defaultClientCacheTokens)]
        public bool CacheIssuedTokens
        {
            get { return (bool)base[ConfigurationStrings.CacheIssuedTokens]; }
            set { base[ConfigurationStrings.CacheIssuedTokens] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxIssuedTokenCachingTime, DefaultValue = SpnegoTokenProvider.defaultClientMaxTokenCachingTimeString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan MaxIssuedTokenCachingTime
        {
            get { return (TimeSpan)base[ConfigurationStrings.MaxIssuedTokenCachingTime]; }
            set { base[ConfigurationStrings.MaxIssuedTokenCachingTime] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.DefaultKeyEntropyMode, DefaultValue = System.ServiceModel.Security.AcceleratedTokenProvider.defaultKeyEntropyMode)]
        [ServiceModelEnumValidator(typeof(SecurityKeyEntropyModeHelper))]
        public SecurityKeyEntropyMode DefaultKeyEntropyMode
        {
            get { return (SecurityKeyEntropyMode)base[ConfigurationStrings.DefaultKeyEntropyMode]; }
            set { base[ConfigurationStrings.DefaultKeyEntropyMode] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.IssuedTokenRenewalThresholdPercentage, DefaultValue = SpnegoTokenProvider.defaultServiceTokenValidityThresholdPercentage)]
        [IntegerValidator(MinValue = 0, MaxValue = 100)]
        public int IssuedTokenRenewalThresholdPercentage
        {
            get { return (int)base[ConfigurationStrings.IssuedTokenRenewalThresholdPercentage]; }
            set { base[ConfigurationStrings.IssuedTokenRenewalThresholdPercentage] = value; }
        }

        public void Copy(IssuedTokenClientElement from)
        {
            if (this.IsReadOnly())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigReadOnly)));
            }
            if (null == from)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("from");
            }

            this.DefaultKeyEntropyMode = from.DefaultKeyEntropyMode;
            this.CacheIssuedTokens = from.CacheIssuedTokens;
            this.MaxIssuedTokenCachingTime = from.MaxIssuedTokenCachingTime;
            this.IssuedTokenRenewalThresholdPercentage = from.IssuedTokenRenewalThresholdPercentage;

#pragma warning suppress 56506 //[....]; from.ElementInformation.Properties[ConfigurationStrings.LocalIssuerIssuedTokenParameters] can never be null (underlying configuration system guarantees)
            if (PropertyValueOrigin.Default != from.ElementInformation.Properties[ConfigurationStrings.LocalIssuer].ValueOrigin)
            {
                this.LocalIssuer.Copy(from.LocalIssuer);
            }
#pragma warning suppress 56506 //[....]; from.ElementInformation.Properties[ConfigurationStrings.LocalIssuerChannelBehaviors] can never be null (underlying configuration system guarantees)
            if (PropertyValueOrigin.Default != from.ElementInformation.Properties[ConfigurationStrings.LocalIssuerChannelBehaviors].ValueOrigin)
            {
                this.LocalIssuerChannelBehaviors = from.LocalIssuerChannelBehaviors;
            }
#pragma warning suppress 56506 //[....]; from.ElementInformation.Properties[ConfigurationStrings.IssuerChannelBehaviors] can never be null (underlying configuration system guarantees)
            if (PropertyValueOrigin.Default != from.ElementInformation.Properties[ConfigurationStrings.IssuerChannelBehaviors].ValueOrigin)
            {
                foreach (IssuedTokenClientBehaviorsElement element in from.IssuerChannelBehaviors)
                {
                    this.IssuerChannelBehaviors.Add(element);
                }
            }
        }

        internal void ApplyConfiguration(IssuedTokenClientCredential issuedToken)
        {
            if (issuedToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("issuedToken");
            }
            issuedToken.CacheIssuedTokens = this.CacheIssuedTokens;
            issuedToken.DefaultKeyEntropyMode = this.DefaultKeyEntropyMode;
            issuedToken.MaxIssuedTokenCachingTime = this.MaxIssuedTokenCachingTime;
            issuedToken.IssuedTokenRenewalThresholdPercentage = this.IssuedTokenRenewalThresholdPercentage;
            if (PropertyValueOrigin.Default != this.ElementInformation.Properties[ConfigurationStrings.LocalIssuer].ValueOrigin)
            {
                this.LocalIssuer.Validate();
                issuedToken.LocalIssuerAddress = ConfigLoader.LoadEndpointAddress(this.LocalIssuer);
                if (!string.IsNullOrEmpty(this.LocalIssuer.Binding))
                {
                    issuedToken.LocalIssuerBinding = ConfigLoader.LookupBinding(this.LocalIssuer.Binding, this.LocalIssuer.BindingConfiguration, this.EvaluationContext);
                }
            }
            if (!string.IsNullOrEmpty(this.LocalIssuerChannelBehaviors))
            {
                ConfigLoader.LoadChannelBehaviors(this.LocalIssuerChannelBehaviors, this.EvaluationContext, issuedToken.LocalIssuerChannelBehaviors);
            }
            if (PropertyValueOrigin.Default != this.ElementInformation.Properties[ConfigurationStrings.IssuerChannelBehaviors].ValueOrigin)
            {
                foreach (IssuedTokenClientBehaviorsElement issuerBehaviorElement in this.IssuerChannelBehaviors)
                {
                    if (!string.IsNullOrEmpty(issuerBehaviorElement.BehaviorConfiguration))
                    {
                        KeyedByTypeCollection<IEndpointBehavior> issuerBehaviors = new KeyedByTypeCollection<IEndpointBehavior>();
                        ConfigLoader.LoadChannelBehaviors(issuerBehaviorElement.BehaviorConfiguration, this.EvaluationContext, issuerBehaviors);
                        issuedToken.IssuerChannelBehaviors.Add(new Uri(issuerBehaviorElement.IssuerAddress), issuerBehaviors);
                    }
                }
            }
        }
    }
}
