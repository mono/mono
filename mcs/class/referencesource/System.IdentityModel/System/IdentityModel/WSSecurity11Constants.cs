//-----------------------------------------------------------------------
// <copyright file="WSSecurity11Constants.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel
{
    /// <summary>
    /// Defines constants used in WS-Security 1.1 standard schema.
    /// </summary>
    internal static class WSSecurity11Constants
    {
#pragma warning disable 1591
        public const string FragmentBaseAddress = "http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1";
        public const string Namespace           = "http://docs.oasis-open.org/wss/oasis-wss-wssecurity-secext-1.1.xsd";
        public const string Prefix              = "wsse11";

        public static class Attributes
        {
            public const string TokenType = "TokenType";
        }

        public static class KeyTypes
        {
            public const string CardSpaceV1Sha1Thumbprint = "http://docs.oasis-open.org/wss/2004/xx/oasis-2004xx-wss-soap-message-security-1.1#ThumbprintSHA1";
            public const string Sha1Thumbprint            = FragmentBaseAddress + "#ThumbprintSHA1";
        }
    }
#pragma warning restore 1591
}
