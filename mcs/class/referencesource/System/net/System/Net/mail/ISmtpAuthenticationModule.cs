//-----------------------------------------------------------------------------
// <copyright file="ISmtpAuthenticationModule.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------

namespace System.Net.Mail
{
    using System;
    using System.Net;
    using System.Security.Authentication.ExtendedProtection;

    internal interface ISmtpAuthenticationModule
    {
        Authorization Authenticate(string challenge, NetworkCredential credentials, object sessionCookie, string spn, ChannelBinding channelBindingToken);
        string AuthenticationType { get; }

        //
        // CloseContext
        //
        // Signal to module to close any security context created in
        // calls to Authenticate.
        //
        // Added to allow for a module to not automatically release a
        // security context upon auth completion.
        //
        // Needed for SMTP AUTH GSSAPI where the security context is used 
        // after authentication completes to verify and construct
        // signed messages.
        //
        // All SMTP auth modules must have an implementation of
        // this function.  It will be called for all modules but
        // those that automatically release the context can ignore it.
        // 
        void CloseContext(object sessionCookie);
    }
}
