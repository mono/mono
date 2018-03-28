//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Globalization;
    using System.IdentityModel.Selectors;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.Web.Security;

    public sealed class UserNamePasswordServiceCredential
    {
        internal const UserNamePasswordValidationMode DefaultUserNamePasswordValidationMode = UserNamePasswordValidationMode.Windows;
        internal const bool DefaultCacheLogonTokens = false;
        internal const int DefaultMaxCachedLogonTokens = 128;
        internal const string DefaultCachedLogonTokenLifetimeString = "00:15:00";
        internal static readonly TimeSpan DefaultCachedLogonTokenLifetime = TimeSpan.Parse(DefaultCachedLogonTokenLifetimeString, CultureInfo.InvariantCulture);

        UserNamePasswordValidationMode validationMode = DefaultUserNamePasswordValidationMode;
        UserNamePasswordValidator validator;
        object membershipProvider;
        bool includeWindowsGroups = SspiSecurityTokenProvider.DefaultExtractWindowsGroupClaims;
        bool cacheLogonTokens = DefaultCacheLogonTokens;
        int maxCachedLogonTokens = DefaultMaxCachedLogonTokens;
        TimeSpan cachedLogonTokenLifetime = DefaultCachedLogonTokenLifetime;
        bool isReadOnly;

        internal UserNamePasswordServiceCredential()
        {
            // empty
        }

        internal UserNamePasswordServiceCredential(UserNamePasswordServiceCredential other)
        {
            this.includeWindowsGroups = other.includeWindowsGroups;
            this.membershipProvider = other.membershipProvider;
            this.validationMode = other.validationMode;
            this.validator = other.validator;
            this.cacheLogonTokens = other.cacheLogonTokens;
            this.maxCachedLogonTokens = other.maxCachedLogonTokens;
            this.cachedLogonTokenLifetime = other.cachedLogonTokenLifetime;
            this.isReadOnly = other.isReadOnly;
        }

        public UserNamePasswordValidationMode UserNamePasswordValidationMode 
        { 
            get 
            {
                return this.validationMode;
            } 
            set 
            {
                UserNamePasswordValidationModeHelper.Validate(value);
                ThrowIfImmutable();
                this.validationMode = value;
            } 
        }

        public UserNamePasswordValidator CustomUserNamePasswordValidator 
        { 
            get 
            {
                return this.validator; 
            } 
            set 
            {
                ThrowIfImmutable();
                this.validator = value;
            } 
        }

        public MembershipProvider MembershipProvider
        {
            get
            {
                return (MembershipProvider)this.membershipProvider;
            }
            set
            {
                ThrowIfImmutable();
                this.membershipProvider = value;
            }
        }

        public bool IncludeWindowsGroups
        {
            get
            {
                return this.includeWindowsGroups;
            }
            set
            {
                ThrowIfImmutable();
                this.includeWindowsGroups = value;
            }
        }

        public bool CacheLogonTokens
        {
            get
            {
                return this.cacheLogonTokens;
            }
            set
            {
                ThrowIfImmutable();
                this.cacheLogonTokens = value;
            }
        }

        public int MaxCachedLogonTokens
        {
            get
            {
                return this.maxCachedLogonTokens;
            }
            set
            {
                if (value <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", SR.GetString(SR.ValueMustBeGreaterThanZero)));
                }
                ThrowIfImmutable();
                this.maxCachedLogonTokens = value;
            }
        }

        public TimeSpan CachedLogonTokenLifetime
        {
            get
            {
                return this.cachedLogonTokenLifetime;
            }
            set
            {
                if (value <= TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", SR.GetString(SR.TimeSpanMustbeGreaterThanTimeSpanZero)));
                }

                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
                }
                ThrowIfImmutable();
                this.cachedLogonTokenLifetime = value;
            }
        }

        internal UserNamePasswordValidator GetUserNamePasswordValidator()
        {
            if (this.validationMode == UserNamePasswordValidationMode.MembershipProvider)
            {
                return this.GetMembershipProviderValidator();
            }
            else if (this.validationMode == UserNamePasswordValidationMode.Custom)
            {
                if (this.validator == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MissingCustomUserNamePasswordValidator)));
                }
                return this.validator;
            }

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        UserNamePasswordValidator GetMembershipProviderValidator()
        {
            MembershipProvider provider;

            if (this.membershipProvider != null)
            {
                provider = (MembershipProvider)this.membershipProvider;
            }
            else
            {
                provider = SystemWebHelper.GetMembershipProvider();
            }

            if (provider == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MissingMembershipProvider)));
            }
            return UserNamePasswordValidator.CreateMembershipProviderValidator(provider);
        }

        internal void MakeReadOnly()
        {
            this.isReadOnly = true;
        }

        void ThrowIfImmutable()
        {
            if (this.isReadOnly)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
            }
        }
    }
}
