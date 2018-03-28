//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    /// <summary>
    /// Defines constants that are used to describe Operation, ServiceContract 
    /// and ServiceBehavior names.
    /// </summary>
    internal static class WSTrustServiceContractConstants
    {
#pragma warning disable 1591
        public const string ServiceBehaviorName = "SecurityTokenService";
        public const string Namespace = "http://schemas.microsoft.com/ws/2008/06/identity/securitytokenservice";

        public static class Contracts
        {
            public const string IWSTrustFeb2005Async = "IWSTrustFeb2005Async";
            public const string IWSTrustFeb2005Sync = "IWSTrustFeb2005Sync";
            public const string IWSTrust13Async = "IWSTrust13Async";
            public const string IWSTrust13Sync = "IWSTrust13Sync";
        }

        public static class Operations
        {
            // IWSTrustFeb2005Async Operations.
            public const string TrustFeb2005CancelAsync = "TrustFeb2005CancelAsync";
            public const string TrustFeb2005CancelResponseAsync = "TrustFeb2005CancelResponseAsync";
            public const string TrustFeb2005IssueAsync = "TrustFeb2005IssueAsync";
            public const string TrustFeb2005IssueResponseAsync = "TrustFeb2005IssueResponseAsync";
            public const string TrustFeb2005RenewAsync = "TrustFeb2005RenewAsync";
            public const string TrustFeb2005RenewResponseAsync = "TrustFeb2005RenewResponseAsync";
            public const string TrustFeb2005ValidateAsync = "TrustFeb2005ValidateAsync";
            public const string TrustFeb2005ValidateResponseAsync = "TrustFeb2005ValidateResponseAsync";

            // IWSTrustFeb2005Sync Operations.
            public const string TrustFeb2005Cancel = "TrustFeb2005Cancel";
            public const string TrustFeb2005CancelResponse = "TrustFeb2005CancelResponse";
            public const string TrustFeb2005Issue = "TrustFeb2005Issue";
            public const string TrustFeb2005IssueResponse = "TrustFeb2005IssueResponse"; 
            public const string TrustFeb2005Renew = "TrustFeb2005Renew";
            public const string TrustFeb2005RenewResponse = "TrustFeb2005RenewResponse";
            public const string TrustFeb2005Validate = "TrustFeb2005Validate";
            public const string TrustFeb2005ValidateResponse = "TrustFeb2005ValidateResponse";

            // IWSTrust13Async Operations.
            public const string Trust13CancelAsync = "Trust13CancelAsync";
            public const string Trust13CancelResponseAsync = "Trust13CancelResponseAsync";
            public const string Trust13IssueAsync = "Trust13IssueAsync";
            public const string Trust13IssueResponseAsync = "Trust13IssueResponseAsync";
            public const string Trust13RenewAsync = "Trust13RenewAsync";
            public const string Trust13RenewResponseAsync = "Trust13RenewResponseAsync";
            public const string Trust13ValidateAsync = "Trust13ValidateAsync";
            public const string Trust13ValidateResponseAsync = "Trust13ValidateResponseAsync";

            // IWSTrust13Sync Operations.
            public const string Trust13Cancel = "Trust13Cancel";
            public const string Trust13CancelResponse = "Trust13CancelResponse";
            public const string Trust13Issue = "Trust13Issue";
            public const string Trust13IssueResponse = "Trust13IssueResponse";
            public const string Trust13Renew = "Trust13Renew";
            public const string Trust13RenewResponse = "Trust13RenewResponse";
            public const string Trust13Validate = "Trust13Validate";
            public const string Trust13ValidateResponse = "Trust13ValidateResponse";
#pragma warning restore 1591
        }
    }
}
