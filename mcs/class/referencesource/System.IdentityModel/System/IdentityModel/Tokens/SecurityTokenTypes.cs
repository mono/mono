//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System.Globalization;

namespace System.IdentityModel.Tokens
{
    public static class SecurityTokenTypes
    {
        const string Namespace = "http://schemas.microsoft.com/ws/2006/05/identitymodel/tokens";
        const string userName = Namespace + "/UserName";
        const string x509Certificate = Namespace + "/X509Certificate";
        const string kerberos = Namespace + "/Kerberos";
        const string saml = Namespace + "/Saml";
        const string rsa = Namespace + "/Rsa";

        internal const string SamlTokenProfile11 = "urn:oasis:names:tc:SAML:1.0:assertion";
        internal const string Saml2TokenProfile11 = "urn:oasis:names:tc:SAML:2.0:assertion";

        internal const string OasisWssSamlTokenProfile11 = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV1.1";
        internal const string OasisWssSaml2TokenProfile11 = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0";

        static public string UserName { get { return userName; } }
        static public string X509Certificate { get { return x509Certificate; } }
        static public string Kerberos { get { return kerberos; } }
        static public string Saml { get { return saml; } }
        static public string Rsa { get { return rsa; } }
    }
}
