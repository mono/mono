//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;

    public sealed partial class AuthorizationPolicyTypeElement : ConfigurationElement
    {
        public AuthorizationPolicyTypeElement()
        {
        }

        public AuthorizationPolicyTypeElement(string policyType)
        {
            if (String.IsNullOrEmpty(policyType))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("policyType");
            }
            this.PolicyType = policyType;
        }

        [ConfigurationProperty(ConfigurationStrings.PolicyType, Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
        [StringValidator(MinLength = 1)]
        public string PolicyType
        {
            get { return (string)base[ConfigurationStrings.PolicyType]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }
                base[ConfigurationStrings.PolicyType] = value;
            }
        }
    }
}
