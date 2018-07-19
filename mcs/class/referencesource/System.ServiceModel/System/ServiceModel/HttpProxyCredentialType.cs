//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System.Net;
    using System.Runtime;

    public enum HttpProxyCredentialType
    {
        None,
        Basic,
        Digest,
        Ntlm,
        Windows,
    }

    static class HttpProxyCredentialTypeHelper
    {
        internal static bool IsDefined(HttpProxyCredentialType value)
        {
            return (value == HttpProxyCredentialType.None ||
                value == HttpProxyCredentialType.Basic ||
                value == HttpProxyCredentialType.Digest ||
                value == HttpProxyCredentialType.Ntlm ||
                value == HttpProxyCredentialType.Windows);
        }

        internal static AuthenticationSchemes MapToAuthenticationScheme(HttpProxyCredentialType proxyCredentialType)
        {
            AuthenticationSchemes result;
            switch (proxyCredentialType)
            {
                case HttpProxyCredentialType.None:
                    result = AuthenticationSchemes.Anonymous;
                    break;
                case HttpProxyCredentialType.Basic:
                    result = AuthenticationSchemes.Basic;
                    break;
                case HttpProxyCredentialType.Digest:
                    result = AuthenticationSchemes.Digest;
                    break;
                case HttpProxyCredentialType.Ntlm:
                    result = AuthenticationSchemes.Ntlm;
                    break;
                case HttpProxyCredentialType.Windows:
                    result = AuthenticationSchemes.Negotiate;
                    break;
                default:
                    Fx.Assert("unsupported proxy credential type");
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
            return result;
        }

        internal static HttpProxyCredentialType MapToProxyCredentialType(AuthenticationSchemes authenticationSchemes)
        {
            HttpProxyCredentialType result;
            switch (authenticationSchemes)
            {
                case AuthenticationSchemes.Anonymous:
                    result = HttpProxyCredentialType.None;
                    break;
                case AuthenticationSchemes.Basic:
                    result = HttpProxyCredentialType.Basic;
                    break;
                case AuthenticationSchemes.Digest:
                    result = HttpProxyCredentialType.Digest;
                    break;
                case AuthenticationSchemes.Ntlm:
                    result = HttpProxyCredentialType.Ntlm;
                    break;
                case AuthenticationSchemes.Negotiate:
                    result = HttpProxyCredentialType.Windows;
                    break;
                default:
                    Fx.Assert("unsupported authentication Scheme");
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
            return result;
        }
    }
}
