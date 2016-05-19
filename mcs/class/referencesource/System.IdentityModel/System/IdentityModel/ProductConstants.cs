//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;

namespace System.IdentityModel
{
    /// <summary>
    /// Intended for internal framework use only.
    /// </summary>
    internal static class ProductConstants
    {
#pragma warning disable 1591
        public const string NamespaceUri = "http://schemas.microsoft.com/ws/2008/06/identity";
        public const string ClaimValueTypeSerializationPrefix = "tn"; // for target namespace
        public const string ClaimValueTypeSerializationPrefixWithColon = "tn:";
        
#pragma warning restore 1591
    }
}
