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
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.ComponentModel;

    public sealed partial class MsmqTransportSecurityElement : ServiceModelConfigurationElement
    {

        [ConfigurationProperty(ConfigurationStrings.MsmqAuthenticationMode, DefaultValue = MsmqDefaults.MsmqAuthenticationMode)]
        [ServiceModelEnumValidator(typeof(MsmqAuthenticationModeHelper))]
        public MsmqAuthenticationMode MsmqAuthenticationMode
        {
            get { return (MsmqAuthenticationMode)base[ConfigurationStrings.MsmqAuthenticationMode]; }
            set { base[ConfigurationStrings.MsmqAuthenticationMode] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MsmqEncryptionAlgorithm, DefaultValue = MsmqDefaults.MsmqEncryptionAlgorithm)]
        [ServiceModelEnumValidator(typeof(MsmqEncryptionAlgorithmHelper))]
        public MsmqEncryptionAlgorithm MsmqEncryptionAlgorithm
        {
            get { return (MsmqEncryptionAlgorithm)base[ConfigurationStrings.MsmqEncryptionAlgorithm]; }
            set { base[ConfigurationStrings.MsmqEncryptionAlgorithm] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MsmqProtectionLevel, DefaultValue = MsmqDefaults.MsmqProtectionLevel)]
        [ServiceModelEnumValidator(typeof(ProtectionLevelHelper))]
        public ProtectionLevel MsmqProtectionLevel
        {
            get { return (ProtectionLevel)base[ConfigurationStrings.MsmqProtectionLevel]; }
            set { base[ConfigurationStrings.MsmqProtectionLevel] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MsmqSecureHashAlgorithm)]
        [ServiceModelEnumValidator(typeof(MsmqSecureHashAlgorithmHelper))]
        public MsmqSecureHashAlgorithm MsmqSecureHashAlgorithm
        {
            get { return (MsmqSecureHashAlgorithm)(base[ConfigurationStrings.MsmqSecureHashAlgorithm] ?? MsmqDefaults.MsmqSecureHashAlgorithm); }
            set { base[ConfigurationStrings.MsmqSecureHashAlgorithm] = value; }
        }

        internal void ApplyConfiguration(MsmqTransportSecurity security)
        {
            if (security == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");

            security.MsmqAuthenticationMode = this.MsmqAuthenticationMode;
            security.MsmqEncryptionAlgorithm = this.MsmqEncryptionAlgorithm;
            security.MsmqProtectionLevel = this.MsmqProtectionLevel;
            security.MsmqSecureHashAlgorithm = this.MsmqSecureHashAlgorithm;
        }

        internal void InitializeFrom(MsmqTransportSecurity security)
        {
            if (security == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");

            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MsmqAuthenticationMode, security.MsmqAuthenticationMode);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MsmqEncryptionAlgorithm, security.MsmqEncryptionAlgorithm);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MsmqProtectionLevel, security.MsmqProtectionLevel);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MsmqSecureHashAlgorithm, security.MsmqSecureHashAlgorithm);            
        }
    }
}
