//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Metadata
{
    /// <summary>
    /// Defines XML element names relevant to WS-Federation metadata.
    /// </summary>
    internal static class FederationMetadataConstants
    {
#pragma warning disable 1591
        public static class Elements
        {
            public const string ClaimTypesOffered = "ClaimTypesOffered";
            public const string ClaimTypesRequested = "ClaimTypesRequested";
            public const string TargetScopes = "TargetScopes";
            public const string TokenTypesOffered = "TokenTypesOffered";

            public const string ApplicationServiceType = "ApplicationServiceType";
            public const string SecurityTokenServiceType = "SecurityTokenServiceType";

            public const string ApplicationServiceEndpoint = "ApplicationServiceEndpoint";
            public const string PassiveRequestorEndpoint = "PassiveRequestorEndpoint";
            public const string SecurityTokenServiceEndpoint = "SecurityTokenServiceEndpoint";
        }

        public const string Namespace = "http://docs.oasis-open.org/wsfed/federation/200706";
        public const string Prefix    = "fed";
    }
#pragma warning restore 1591
}
