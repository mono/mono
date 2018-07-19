//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System;

    public static class SamlConstants
    {
        static public int MajorVersionValue { get { return 1; } }
        static public int MinorVersionValue { get { return 1; } }
        static public string Namespace { get { return SamlStrings.Namespace; } }
        static public string HolderOfKey { get { return SamlStrings.HolderOfKey; } }
        static public string SenderVouches { get { return SamlStrings.SenderVouches; } }
        static public string UserName { get { return SamlStrings.UserName; } }
        static public string UserNameNamespace { get { return SamlStrings.UserNameNamespace; } }
        static public string EmailName { get { return SamlStrings.EmailName; } }
        static public string EmailNamespace { get { return SamlStrings.EmailNamespace; } }

        public const string Prefix = "saml";

        internal static string[] AcceptedDateTimeFormats = new string[] {
                "yyyy-MM-ddTHH:mm:ss.fffffffZ",
                "yyyy-MM-ddTHH:mm:ss.ffffffZ",
                "yyyy-MM-ddTHH:mm:ss.fffffZ",
                "yyyy-MM-ddTHH:mm:ss.ffffZ",
                "yyyy-MM-ddTHH:mm:ss.fffZ",
                "yyyy-MM-ddTHH:mm:ss.ffZ",
                "yyyy-MM-ddTHH:mm:ss.fZ",
                "yyyy-MM-ddTHH:mm:ssZ",
                "yyyy-MM-ddTHH:mm:ss.fffffffzzz",
                "yyyy-MM-ddTHH:mm:ss.ffffffzzz",
                "yyyy-MM-ddTHH:mm:ss.fffffzzz",
                "yyyy-MM-ddTHH:mm:ss.ffffzzz",
                "yyyy-MM-ddTHH:mm:ss.fffzzz",
                "yyyy-MM-ddTHH:mm:ss.ffzzz",
                "yyyy-MM-ddTHH:mm:ss.fzzz",
                "yyyy-MM-ddTHH:mm:sszzz" };
        internal const string AssertionIdPrefix = "SamlSecurityToken-";
        internal const string GeneratedDateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";


        /// <summary>
        /// Known values for <see cref="System.IdentityModel.Tokens.SamlAuthenticationStatement"/>
        /// </summary>
        internal static class AuthenticationMethods
        {
            public const string HardwareTokenString = "URI:urn:oasis:names:tc:SAML:1.0:am:HardwareToken";
            public const string KerberosString = "urn:ietf:rfc:1510";
            public const string PasswordString = "urn:oasis:names:tc:SAML:1.0:am:password";
            public const string PgpString = "urn:oasis:names:tc:SAML:1.0:am:PGP";
            public const string SecureRemotePasswordString = "urn:ietf:rfc:2945";
            public const string SignatureString = "urn:ietf:rfc:3075";
            public const string SpkiString = "urn:oasis:names:tc:SAML:1.0:am:SPKI";
            public const string TlsClientString = "urn:ietf:rfc:2246";
            public const string UnspecifiedString = "urn:oasis:names:tc:SAML:1.0:am:unspecified";
            public const string WindowsString = "urn:federation:authentication:windows";
            public const string X509String = "urn:oasis:names:tc:SAML:1.0:am:X509-PKI";
            public const string XkmsString = "urn:oasis:names:tc:SAML:1.0:am:XKMS";
        }

        internal static class ElementNames
        {
            public const string Action = "Action";
            public const string Advice = "Advice";
            public const string Assertion = "Assertion";
            public const string AssertionIdReference = "AssertionIDReference";
            public const string Attribute = "Attribute";
            public const string AttributeStatement = "AttributeStatement";
            public const string AttributeValue = "AttributeValue";
            public const string Audience = "Audience";
            public const string AudienceRestrictionCondition = "AudienceRestrictionCondition";
            public const string AuthenticationStatement = "AuthenticationStatement";
            public const string AuthorityBinding = "AuthorityBinding";
            public const string AuthorizationDecisionStatement = "AuthorizationDecisionStatement";
            public const string Conditions = "Conditions";
            public const string DoNotCacheCondition = "DoNotCacheCondition";
            public const string Evidence = "Evidence";
            public const string NameIdentifier = "NameIdentifier";
            public const string SubjectConfirmation = "SubjectConfirmation";
            public const string Subject = "Subject";
            public const string SubjectConfirmationData = "SubjectConfirmationData";
            public const string SubjectConfirmationMethod = "ConfirmationMethod";
            public const string SubjectLocality = "SubjectLocality";
        }

        internal static class AttributeNames
        {
            public const string AssertionId = "AssertionID";
            public const string AttributeName = "AttributeName";
            public const string AttributeNamespace = "AttributeNamespace";
            public const string AuthenticationInstant = "AuthenticationInstant";
            public const string AuthenticationMethod = "AuthenticationMethod";
            public const string AuthorityBinding = "AuthorityBinding";
            public const string AuthorityKind = "AuthorityKind";
            public const string Binding = "Binding";
            public const string Decision = "Decision";
            public const string Issuer = "Issuer";
            public const string IssueInstant = "IssueInstant";
            public const string Location = "Location";
            public const string MajorVersion = "MajorVersion";
            public const string MinorVersion = "MinorVersion";
            public const string OriginalIssuer = "OriginalIssuer";
            public const string NamespaceAttributePrefix = "xmlns";
            public const string NameIdentifierFormat = "Format";
            public const string NameIdentifierNameQualifier = "NameQualifier";
            public const string Namespace = "Namespace";
            public const string NotBefore = "NotBefore";
            public const string NotOnOrAfter = "NotOnOrAfter";
            public const string Resource = "Resource";
            public const string SubjectLocalityDNSAddress = "DNSAddress";
            public const string SubjectLocalityIPAddress = "IPAddress";
        }
    }
}
