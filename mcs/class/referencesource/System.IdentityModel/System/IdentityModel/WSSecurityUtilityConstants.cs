//-----------------------------------------------------------------------
// <copyright file="WSSecurityUtilityConstants.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel
{
    /// <summary>
    /// Defines constants used in WS-SecureUtility standard schema.
    /// </summary>
    internal static class WSSecurityUtilityConstants
    {
#pragma warning disable 1591
        public const string Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
        public const string Prefix    = "wsu";

        public static class Attributes
        {
            public const string Id = "Id";
        }
#pragma warning restore 1591
    }
}
