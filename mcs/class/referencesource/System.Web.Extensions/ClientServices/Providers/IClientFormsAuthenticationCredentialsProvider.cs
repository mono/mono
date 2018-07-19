//------------------------------------------------------------------------------
// <copyright file="IClientFormsAuthenticationCredentialsProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Web.ClientServices.Providers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    public interface IClientFormsAuthenticationCredentialsProvider
    {
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification="Reviewed and approved by feature crew")]
        ClientFormsAuthenticationCredentials GetCredentials();
    }
}
