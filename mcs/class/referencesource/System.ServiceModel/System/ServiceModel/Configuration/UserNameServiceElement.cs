//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.IdentityModel.Selectors;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Security;

    public sealed partial class UserNameServiceElement : ConfigurationElement
    {
        public UserNameServiceElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.UserNamePasswordValidationMode, DefaultValue = UserNamePasswordServiceCredential.DefaultUserNamePasswordValidationMode)]
        [ServiceModelEnumValidator(typeof(UserNamePasswordValidationModeHelper))]
        public UserNamePasswordValidationMode UserNamePasswordValidationMode
        {
            get { return (UserNamePasswordValidationMode)base[ConfigurationStrings.UserNamePasswordValidationMode]; }
            set { base[ConfigurationStrings.UserNamePasswordValidationMode] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.IncludeWindowsGroups, DefaultValue = SspiSecurityTokenProvider.DefaultExtractWindowsGroupClaims)]
        public bool IncludeWindowsGroups
        {
            get { return (bool)base[ConfigurationStrings.IncludeWindowsGroups]; }
            set { base[ConfigurationStrings.IncludeWindowsGroups] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MembershipProviderName, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string MembershipProviderName
        {
            get { return (string)base[ConfigurationStrings.MembershipProviderName]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }
                base[ConfigurationStrings.MembershipProviderName] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.CustomUserNamePasswordValidatorType, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string CustomUserNamePasswordValidatorType
        {
            get { return (string)base[ConfigurationStrings.CustomUserNamePasswordValidatorType]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }
                base[ConfigurationStrings.CustomUserNamePasswordValidatorType] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.CacheLogonTokens, DefaultValue = UserNamePasswordServiceCredential.DefaultCacheLogonTokens)]
        public bool CacheLogonTokens
        {
            get { return (bool)base[ConfigurationStrings.CacheLogonTokens]; }
            set { base[ConfigurationStrings.CacheLogonTokens] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxCachedLogonTokens, DefaultValue = UserNamePasswordServiceCredential.DefaultMaxCachedLogonTokens)]
        [IntegerValidator(MinValue = 1)]
        public int MaxCachedLogonTokens
        {
            get { return (int)base[ConfigurationStrings.MaxCachedLogonTokens]; }
            set { base[ConfigurationStrings.MaxCachedLogonTokens] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.CachedLogonTokenLifetime, DefaultValue = UserNamePasswordServiceCredential.DefaultCachedLogonTokenLifetimeString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanOneTick)]
        public TimeSpan CachedLogonTokenLifetime
        {
            get { return (TimeSpan)base[ConfigurationStrings.CachedLogonTokenLifetime]; }
            set { base[ConfigurationStrings.CachedLogonTokenLifetime] = value; }
        }

        public void Copy(UserNameServiceElement from)
        {
            if (this.IsReadOnly())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigReadOnly)));
            }
            if (null == from)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("from");
            }
            this.UserNamePasswordValidationMode = from.UserNamePasswordValidationMode;
            this.IncludeWindowsGroups = from.IncludeWindowsGroups;
            this.MembershipProviderName = from.MembershipProviderName;
            this.CustomUserNamePasswordValidatorType = from.CustomUserNamePasswordValidatorType;
            this.CacheLogonTokens = from.CacheLogonTokens;
            this.MaxCachedLogonTokens = from.MaxCachedLogonTokens;
            this.CachedLogonTokenLifetime = from.CachedLogonTokenLifetime;
        }

        internal void ApplyConfiguration(UserNamePasswordServiceCredential userName)
        {
            if (userName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("userName");
            }

            userName.UserNamePasswordValidationMode = this.UserNamePasswordValidationMode;
            userName.IncludeWindowsGroups = this.IncludeWindowsGroups;
            userName.CacheLogonTokens = this.CacheLogonTokens;
            userName.MaxCachedLogonTokens = this.MaxCachedLogonTokens;
            userName.CachedLogonTokenLifetime = this.CachedLogonTokenLifetime;
            PropertyInformationCollection propertyInfo = this.ElementInformation.Properties;
            if (propertyInfo[ConfigurationStrings.MembershipProviderName].ValueOrigin != PropertyValueOrigin.Default)
            {
                userName.MembershipProvider = SystemWebHelper.GetMembershipProvider(this.MembershipProviderName);
                if (userName.MembershipProvider == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.InvalidMembershipProviderSpecifiedInConfig, this.MembershipProviderName)));
            }
            else if (userName.UserNamePasswordValidationMode == UserNamePasswordValidationMode.MembershipProvider)
            {
                userName.MembershipProvider = SystemWebHelper.GetMembershipProvider();
            }
            if (!string.IsNullOrEmpty(this.CustomUserNamePasswordValidatorType))
            {
                Type validatorType = System.Type.GetType(this.CustomUserNamePasswordValidatorType, true);
                if (!typeof(UserNamePasswordValidator).IsAssignableFrom(validatorType))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(
                        SR.GetString(SR.ConfigInvalidUserNamePasswordValidatorType, this.CustomUserNamePasswordValidatorType, typeof(UserNamePasswordValidator).ToString())));
                }
                userName.CustomUserNamePasswordValidator = (UserNamePasswordValidator)Activator.CreateInstance(validatorType);
            }
        }
    }
}



