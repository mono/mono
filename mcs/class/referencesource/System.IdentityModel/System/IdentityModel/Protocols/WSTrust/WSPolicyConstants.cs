//-----------------------------------------------------------------------
// <copyright file="WSPolicyConstants.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Protocols.WSTrust
{
    /// <summary>
    /// Defines WS-Policy constants.
    /// </summary>
    internal static class WSPolicyConstants
    {
#pragma warning disable 1591
        public const string NamespaceURI = "http://schemas.xmlsoap.org/ws/2004/09/policy";
        public const string Prefix = "wsp";

        public static class ElementNames
        {
            public const string AppliesTo = "AppliesTo";
        }
#pragma warning restore 1591
    }
}
