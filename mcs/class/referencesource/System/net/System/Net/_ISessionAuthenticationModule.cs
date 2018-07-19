//------------------------------------------------------------------------------
// <copyright file="_ICompleteAuthenticationModule.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------


namespace System.Net {

    internal interface ISessionAuthenticationModule : IAuthenticationModule {

        bool Update(string challenge, WebRequest webRequest);

        void ClearSession(WebRequest webRequest);

        bool CanUseDefaultCredentials { get; }

    } // interface ISessionAuthenticationModule


} // namespace System.Net
