//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    using System.IdentityModel.Tokens;
    using System.Web.Security;

    public abstract class UserNamePasswordValidator
    {
        static UserNamePasswordValidator none;

        public static UserNamePasswordValidator None
        {
            get
            {
                if (none == null)
                    none = new NoneUserNamePasswordValidator();
                return none;
            }
        }

        public static UserNamePasswordValidator CreateMembershipProviderValidator(MembershipProvider provider)
        {
            if (provider == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("provider");
            return new MembershipProviderValidator(provider);
        }

        public abstract void Validate(string userName, string password);

        class NoneUserNamePasswordValidator : UserNamePasswordValidator
        {
            public override void Validate(string userName, string password)
            {
            }
        }

        class MembershipProviderValidator : UserNamePasswordValidator
        {
            MembershipProvider provider;

            public MembershipProviderValidator(MembershipProvider provider)
            {
                this.provider = provider;
            }

            public override void Validate(string userName, string password)
            {
                if (!this.provider.ValidateUser(userName, password))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenValidationException(
                        SR.GetString(SR.UserNameAuthenticationFailed, this.provider.GetType().Name)));
                }
            }
        }
    }
}
