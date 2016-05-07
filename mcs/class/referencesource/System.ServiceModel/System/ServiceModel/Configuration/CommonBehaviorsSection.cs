//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.Runtime;
    using System.Security;

    public partial class CommonBehaviorsSection : ConfigurationSection
    {
        [ConfigurationProperty(ConfigurationStrings.EndpointBehaviors, Options = ConfigurationPropertyOptions.None)]
        public CommonEndpointBehaviorElement EndpointBehaviors
        {
            get { return (CommonEndpointBehaviorElement)base[ConfigurationStrings.EndpointBehaviors]; }
        }

        [ConfigurationProperty(ConfigurationStrings.ServiceBehaviors, Options = ConfigurationPropertyOptions.None)]
        public CommonServiceBehaviorElement ServiceBehaviors
        {
            get { return (CommonServiceBehaviorElement)base[ConfigurationStrings.ServiceBehaviors]; }
        }

        internal static CommonBehaviorsSection GetSection()
        {
            return (CommonBehaviorsSection)ConfigurationHelpers.GetSection(ConfigurationStrings.CommonBehaviorsSectionPath);
        }

        [Fx.Tag.SecurityNote(Critical = "Calls Critical method UnsafeGetSection which elevates in order to fetch config."
            + "Caller must guard access to resultant config section.")]
        [SecurityCritical]
        internal static CommonBehaviorsSection UnsafeGetSection()
        {
            return (CommonBehaviorsSection)ConfigurationHelpers.UnsafeGetSection(ConfigurationStrings.CommonBehaviorsSectionPath);
        }

        [Fx.Tag.SecurityNote(Critical = "Calls Critical method UnsafeGetAssociatedSection which elevates in order to fetch config."
            + "Caller must guard access to resultant config section.")]
        [SecurityCritical]
        internal static CommonBehaviorsSection UnsafeGetAssociatedSection(ContextInformation contextEval)
        {
            return (CommonBehaviorsSection)ConfigurationHelpers.UnsafeGetAssociatedSection(contextEval, ConfigurationStrings.CommonBehaviorsSectionPath);
        }
    }
}



