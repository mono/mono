//-----------------------------------------------------------------------
// <copyright file="WSSecureConversation13Constants.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel
{
    /// <summary>
    /// Defines constants used in WS-SecureConversation standard schema.
    /// </summary>
    internal static class WSSecureConversation13Constants
    {
#pragma warning disable 1591
        public const string Namespace = "http://docs.oasis-open.org/ws-sx/ws-secureconversation/200512";
        public static readonly Uri NamespaceUri = new Uri( Namespace );
        public const string Prefix = "sc";
        public const string TokenTypeURI = "http://docs.oasis-open.org/ws-sx/ws-secureconversation/200512/sct";

        public const int DefaultDerivedKeyLength = 32;

        public static class ElementNames
        {
            public const string Name = "SecurityContextToken";
            public const string Identifier = "Identifier";
            public const string Instance = "Instance";
        }

        public static class Attributes
        {
            public const string Length = "Length";
            public const string Nonce  = "Nonce";
            public const string Instance = "Instance";
        }
#pragma warning restore 1591
    }
}
