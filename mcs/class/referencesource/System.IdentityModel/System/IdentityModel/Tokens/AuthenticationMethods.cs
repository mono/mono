//------------------------------------------------------------------------------
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    /// <summary>
    /// Defines constants for SAML authentication methods.
    /// </summary>
    public static class AuthenticationMethods
    {
#pragma warning disable 1591
        public const string Namespace = "http://schemas.microsoft.com/ws/2008/06/identity/authenticationmethod/";

        public const string HardwareToken           = Namespace + "hardwaretoken";
        public const string Kerberos                = Namespace + "kerberos";
        public const string Password                = Namespace + "password";
        public const string Pgp                     = Namespace + "pgp";
        public const string SecureRemotePassword    = Namespace + "secureremotepassword";
        public const string Signature               = Namespace + "signature";
        public const string Smartcard               = Namespace + "smartcard";
        public const string SmartcardPki            = Namespace + "smartcardpki";
        public const string Spki                    = Namespace + "spki";
        public const string TlsClient               = Namespace + "tlsclient";
        public const string Unspecified             = Namespace + "unspecified";
        public const string Windows                 = Namespace + "windows";
        public const string Xkms                    = Namespace + "xkms";
        public const string X509                    = Namespace + "x509";
#pragma warning restore 1591
    }
}
