//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.ComponentModel;
    using System.Net;
    using System.Runtime;
    using System.Text;

    static class AuthenticationSchemesHelper
    {
        public static bool DoesAuthTypeMatch(AuthenticationSchemes authScheme, string authType)
        {
            if ((authType == null) || (authType.Length == 0))
            {
                return authScheme.IsSet(AuthenticationSchemes.Anonymous);
            }

            if (authType.Equals("kerberos", StringComparison.OrdinalIgnoreCase) ||
                authType.Equals("negotiate", StringComparison.OrdinalIgnoreCase))
            {
                return authScheme.IsSet(AuthenticationSchemes.Negotiate);
            }
            else if (authType.Equals("ntlm", StringComparison.OrdinalIgnoreCase))
            {
                return authScheme.IsSet(AuthenticationSchemes.Negotiate) ||
                    authScheme.IsSet(AuthenticationSchemes.Ntlm);
            }

            AuthenticationSchemes authTypeScheme;
            if (!Enum.TryParse<AuthenticationSchemes>(authType, true, out authTypeScheme))
            {
                return false;
            }

            return authScheme.IsSet(authTypeScheme);
        }

        public static bool IsSingleton(this AuthenticationSchemes v)
        {
            bool result;
            switch (v)
            {
                case AuthenticationSchemes.Digest:
                case AuthenticationSchemes.Negotiate:
                case AuthenticationSchemes.Ntlm:
                case AuthenticationSchemes.Basic:
                case AuthenticationSchemes.Anonymous:
                    result = true;
                    break;
                default:
                    result = false;
                    break;
            }
            return result;
        }

        public static bool IsSet(this AuthenticationSchemes thisPtr, AuthenticationSchemes authenticationSchemes)
        {
            return (thisPtr & authenticationSchemes) == authenticationSchemes;
        }

        public static bool IsNotSet(this AuthenticationSchemes thisPtr, AuthenticationSchemes authenticationSchemes)
        {
            return (thisPtr & authenticationSchemes) == 0;
        }

        internal static string ToString(AuthenticationSchemes authScheme)
        {
            return authScheme.ToString().ToLowerInvariant();
        }
    }
}
