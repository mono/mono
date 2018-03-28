//-----------------------------------------------------------------------
// <copyright file="RequestTypes.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Protocols.WSTrust
{
    /// <summary>
    /// This classes defines the protocol aganostic RequestType strings.  
    /// </summary>
    public static class RequestTypes 
    {
#pragma warning disable 1591
        public const string Cancel = "http://schemas.microsoft.com/idfx/requesttype/cancel";
        public const string Issue = "http://schemas.microsoft.com/idfx/requesttype/issue";
        public const string Renew = "http://schemas.microsoft.com/idfx/requesttype/renew";
        public const string Validate = "http://schemas.microsoft.com/idfx/requesttype/validate";

        public const string IssueCard = "http://schemas.microsoft.com/idfx/requesttype/issueCard";
        public const string GetMetadata = "http://schemas.microsoft.com/idfx/requesttype/getMetadata";
#pragma warning restore 1591
    }
}
