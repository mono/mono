//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;

    [ConfigurationCollection(typeof(AuthorizationPolicyTypeElement))]
    public sealed class AuthorizationPolicyTypeElementCollection : ServiceModelConfigurationElementCollection<AuthorizationPolicyTypeElement>
    {
        public AuthorizationPolicyTypeElementCollection()
            : base()
        { }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            AuthorizationPolicyTypeElement authorizationPolicyTypeElement = (AuthorizationPolicyTypeElement)element;
            return authorizationPolicyTypeElement.PolicyType;
        }
    }
}
