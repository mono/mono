//-----------------------------------------------------------------------
// <copyright file="WSUtilityConstants.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel
{
    /// <summary>
    /// Defines constants from WS-Utility specification
    /// </summary>
    internal static class WSUtilityConstants
    {
#pragma warning disable 1591
        public const string NamespaceURI = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
        public const string Prefix = "wsu";

        public static class Attributes
        {
            public const string IdAttribute = "Id";            
        }

        public static class ElementNames
        {
            public const string Created = "Created";
            public const string Expires = "Expires";
#pragma warning restore 1591
        }
    }
}
