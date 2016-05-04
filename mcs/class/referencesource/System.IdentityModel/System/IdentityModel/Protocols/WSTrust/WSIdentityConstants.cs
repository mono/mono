//-----------------------------------------------------------------------
// <copyright file="WSIdentityConstants.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Protocols.WSTrust
{
    using System;
  
    /// <summary>
    /// Defines the Identity Constants.
    /// </summary>
    internal static class WSIdentityConstants
    {
#pragma warning disable 1591
        public const string Namespace = "http://schemas.xmlsoap.org/ws/2005/05/identity";
        public const string Prefix = "i";
        public const string Dialect   = Namespace;

        public static class Attributes
        {
            public const string Optional = "Optional";
            public const string Uri = "Uri";
        }

        public static class Elements
        {
            public const string ClaimType = "ClaimType";
        }
#pragma warning restore 1591
    }
} 
