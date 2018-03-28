//-----------------------------------------------------------------------
// <copyright file="WSAuthorizationConstants.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Protocols.WSFederation
{
    /// <summary>
    /// Defines WS-Federation Authorization constants.
    /// </summary>
    internal static class WSAuthorizationConstants
    {
#pragma warning disable 1591
        public const string Prefix = "auth";
        public const string Namespace        = "http://docs.oasis-open.org/wsfed/authorization/200706";
        public const string Dialect          = Namespace + "/authclaims";
        public const string Action           = Namespace + "/claims/action";

        public static class Attributes
        {
            public const string Name        = "Name";
            public const string Scope       = "Scope";
        }

        public static class Elements
        {
            public const string AdditionalContext   = "AdditionalContext";
            public const string ClaimType           = "ClaimType";
            public const string ContextItem         = "ContextItem";
            public const string Description         = "Description";
            public const string DisplayName         = "DisplayName";
            public const string Value               = "Value";
        }
#pragma warning restore 1591
    }
}
