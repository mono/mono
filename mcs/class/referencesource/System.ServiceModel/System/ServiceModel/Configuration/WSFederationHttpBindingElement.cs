//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel;
    using System.Globalization;
    using System.ServiceModel.Security;
    using System.ComponentModel;
    using System.Text;
    using System.ServiceModel.Channels;

    public partial class WSFederationHttpBindingElement : WSHttpBindingBaseElement
    {
        public WSFederationHttpBindingElement(string name)
            : base(name)
        {
        }

        public WSFederationHttpBindingElement()
            : this(null)
        {
        }

        protected override Type BindingElementType
        {
            get { return typeof(WSFederationHttpBinding); }
        }

        [ConfigurationProperty(ConfigurationStrings.PrivacyNoticeAt, DefaultValue = null)]
        public Uri PrivacyNoticeAt
        {
            get { return (Uri) base[ConfigurationStrings.PrivacyNoticeAt]; }
            set { base[ConfigurationStrings.PrivacyNoticeAt] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.PrivacyNoticeVersion, DefaultValue = 0)]
        [IntegerValidator(MinValue = 0)]
        public int PrivacyNoticeVersion
        {
            get { return (int) base[ConfigurationStrings.PrivacyNoticeVersion]; }
            set { base[ConfigurationStrings.PrivacyNoticeVersion] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.Security)]
        public WSFederationHttpSecurityElement Security
        {
            get { return (WSFederationHttpSecurityElement)base[ConfigurationStrings.Security]; }
        }

        protected internal override void InitializeFrom(Binding binding)
        {
            base.InitializeFrom(binding);
            WSFederationHttpBinding wspBinding = (WSFederationHttpBinding)binding;
            if ( wspBinding.PrivacyNoticeAt != null )
            {
                SetPropertyValueIfNotDefaultValue(ConfigurationStrings.PrivacyNoticeAt, wspBinding.PrivacyNoticeAt);
                SetPropertyValueIfNotDefaultValue(ConfigurationStrings.PrivacyNoticeVersion, wspBinding.PrivacyNoticeVersion);
            }
            this.Security.InitializeFrom(wspBinding.Security);
        }

        protected override void OnApplyConfiguration(Binding binding)
        {
            base.OnApplyConfiguration(binding);
            WSFederationHttpBinding wspBinding = (WSFederationHttpBinding)binding;
            if (this.PrivacyNoticeAt != null)
            {
                wspBinding.PrivacyNoticeAt = this.PrivacyNoticeAt;
                wspBinding.PrivacyNoticeVersion = this.PrivacyNoticeVersion;
            }

            this.Security.ApplyConfiguration(wspBinding.Security);
        }
    }

}
