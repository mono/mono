//-----------------------------------------------------------------------
// <copyright file="WSSecureConversationFeb2005Constants.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel
{
    /// <summary>
    /// Defines constants used in WS-SecureConversation (Feb 2005) standard schema.
    /// </summary>
    internal static class WSSecureConversationFeb2005Constants
    {
#pragma warning disable 1591
        public const string Namespace = "http://schemas.xmlsoap.org/ws/2005/02/sc";
        public static readonly Uri NamespaceUri = new Uri( Namespace );
        public const string Prefix = "sc";
        public const string TokenTypeURI = "http://schemas.xmlsoap.org/ws/2005/02/sc/sct";

        public const int DefaultDerivedKeyLength = 32;

        public static class ElementNames
        {
            public const string Name = "SecurityContextToken";
            public const string Identifier = "Identifier";
            public const string Instance = "Instance";
        }

        public static class Attributes
        {
            // Length isn't actually in SC-Feb2005, but it is in OASIS SC 1.3
            public const string Length = "Length";
            public const string Nonce  = "Nonce";
            public const string Instance = "Instance";
        }
#pragma warning restore 1591
    }
}
