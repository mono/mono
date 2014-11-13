//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.Globalization;
    using System.Net;
    using System.Net.Security;
    using System.ServiceModel;
    using System.ComponentModel;
    using System.ServiceModel.PeerResolvers;

    public sealed partial class PeerResolverElement : ServiceModelConfigurationElement
    {
        [ConfigurationProperty(ConfigurationStrings.Mode, DefaultValue = PeerResolverMode.Auto)]
        [ServiceModelEnumValidator(typeof(PeerResolverModeHelper))]
        public PeerResolverMode Mode
        {
            get { return (PeerResolverMode)base[ConfigurationStrings.Mode]; }
            set { base[ConfigurationStrings.Mode] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ReferralPolicy, DefaultValue = PeerReferralPolicy.Service)]
        [ServiceModelEnumValidator(typeof(PeerReferralPolicyHelper))]
        public PeerReferralPolicy ReferralPolicy
        {
            get { return (PeerReferralPolicy)base[ConfigurationStrings.ReferralPolicy]; }
            set { base[ConfigurationStrings.ReferralPolicy] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.Custom)]
        public PeerCustomResolverElement Custom
        {
            get { return (PeerCustomResolverElement)base[ConfigurationStrings.Custom]; }
        }

        internal void ApplyConfiguration(PeerResolverSettings settings)
        {
            if (settings == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("settings");
            }
            settings.Mode = this.Mode;
            settings.ReferralPolicy = this.ReferralPolicy;
            this.Custom.ApplyConfiguration(settings.Custom);
        }

        internal void InitializeFrom(PeerResolverSettings settings)
        {
            if (settings == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("settings");
            }
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.Mode, settings.Mode);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ReferralPolicy, settings.ReferralPolicy);
            this.Custom.InitializeFrom(settings.Custom);
        }
    }
}

