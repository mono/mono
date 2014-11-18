//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens;

namespace System.Security.Claims
{
    internal static class AuthenticationTypeMaps
    {
        public struct Mapping
        {
            public Mapping( string normalized, string unnormalized )
            {
                Normalized = normalized;
                Unnormalized = unnormalized;
            }

            public string Normalized;
            public string Unnormalized;
        }

        public static Mapping[] Saml = new Mapping[]
        {
                new Mapping( AuthenticationMethods.HardwareToken,          SamlConstants.AuthenticationMethods.HardwareTokenString ),
                new Mapping( AuthenticationMethods.Kerberos,               SamlConstants.AuthenticationMethods.KerberosString ),
                new Mapping( AuthenticationMethods.Password,               SamlConstants.AuthenticationMethods.PasswordString ),
                new Mapping( AuthenticationMethods.Pgp,                    SamlConstants.AuthenticationMethods.PgpString ),
                new Mapping( AuthenticationMethods.SecureRemotePassword,   SamlConstants.AuthenticationMethods.SecureRemotePasswordString ),
                new Mapping( AuthenticationMethods.Signature,              SamlConstants.AuthenticationMethods.SignatureString ),
                new Mapping( AuthenticationMethods.Spki,                   SamlConstants.AuthenticationMethods.SpkiString ),
                new Mapping( AuthenticationMethods.TlsClient,              SamlConstants.AuthenticationMethods.TlsClientString ),
                new Mapping( AuthenticationMethods.Unspecified,            SamlConstants.AuthenticationMethods.UnspecifiedString ),
                new Mapping( AuthenticationMethods.Windows,                SamlConstants.AuthenticationMethods.WindowsString ),
                new Mapping( AuthenticationMethods.X509,                   SamlConstants.AuthenticationMethods.X509String ),
                new Mapping( AuthenticationMethods.Xkms,                   SamlConstants.AuthenticationMethods.XkmsString ),
        };

        public static Mapping[] Saml2 = new Mapping[]
        {
                new Mapping( AuthenticationMethods.Kerberos,               Saml2Constants.AuthenticationContextClasses.KerberosString ),
                new Mapping( AuthenticationMethods.Password,               Saml2Constants.AuthenticationContextClasses.PasswordString ),
                new Mapping( AuthenticationMethods.Pgp,                    Saml2Constants.AuthenticationContextClasses.PgpString ),
                new Mapping( AuthenticationMethods.SecureRemotePassword,   Saml2Constants.AuthenticationContextClasses.SecureRemotePasswordString ),
                new Mapping( AuthenticationMethods.Signature,              Saml2Constants.AuthenticationContextClasses.XmlDsigString ),
                new Mapping( AuthenticationMethods.Spki,                   Saml2Constants.AuthenticationContextClasses.SpkiString ),
                new Mapping( AuthenticationMethods.Smartcard,              Saml2Constants.AuthenticationContextClasses.SmartcardString ),
                new Mapping( AuthenticationMethods.SmartcardPki,           Saml2Constants.AuthenticationContextClasses.SmartcardPkiString ),
                new Mapping( AuthenticationMethods.TlsClient,              Saml2Constants.AuthenticationContextClasses.TlsClientString ),
                new Mapping( AuthenticationMethods.Unspecified,            Saml2Constants.AuthenticationContextClasses.UnspecifiedString ),
                new Mapping( AuthenticationMethods.X509,                   Saml2Constants.AuthenticationContextClasses.X509String ),
                new Mapping( AuthenticationMethods.Windows,                Saml2Constants.AuthenticationContextClasses.WindowsString ),
        };

        /// <summary>
        /// Returns the protocol specific value matching a normalized value.
        /// </summary>
        /// <remarks>
        /// If no match is found, the original value is returned.
        /// </remarks>
        public static string Denormalize( string normalizedAuthenticationMethod, Mapping[] mappingTable )
        {
            foreach ( Mapping mapping in mappingTable )
            {
                if (StringComparer.Ordinal.Equals( normalizedAuthenticationMethod, mapping.Normalized ) )
                    return mapping.Unnormalized;
            }

            return normalizedAuthenticationMethod;
        }

        /// <summary>
        /// Returns the normalized value matching a protocol specific value.
        /// </summary>
        /// <remarks>
        /// If no match is found, the original value is returned.
        /// </remarks>
        public static string Normalize( string unnormalizedAuthenticationMethod, Mapping[] mappingTable )
        {
            foreach ( Mapping mapping in mappingTable )
            {
                if ( StringComparer.Ordinal.Equals( unnormalizedAuthenticationMethod, mapping.Unnormalized ) )
                    return mapping.Normalized;
            }

            return unnormalizedAuthenticationMethod;
        }
    }
}
