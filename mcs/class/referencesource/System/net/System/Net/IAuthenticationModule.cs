//------------------------------------------------------------------------------
// <copyright file="IAuthenticationModule.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------


namespace System.Net {
    /// <devdoc>
    ///    <para>Provides the base authentication interface for Web client authentication
    ///       modules.</para>
    /// </devdoc>
    public interface IAuthenticationModule {
        /// <devdoc>
        /// <para>Returns an instance of the <see cref='System.Net.Authorization'/>
        /// class in response to the
        /// authentication challenge from a server.</para>
        /// </devdoc>
        Authorization Authenticate(string challenge, WebRequest request, ICredentials credentials);

        /// <devdoc>
        /// <para>Returns an instance of the <see cref='System.Net.Authorization'/> class in response to an
        ///    authentication request to a server.</para>
        /// </devdoc>
        Authorization PreAuthenticate(WebRequest request, ICredentials credentials);

        /// <devdoc>
        ///    <para>
        ///       Indicates whether the module implementing the interface supports
        ///       pre-authentication.
        ///    </para>
        /// </devdoc>
        bool CanPreAuthenticate { get;}

        /// <devdoc>
        ///    <para>
        ///       The authentication scheme used by the host.
        ///       .
        ///    </para>
        /// </devdoc>
        string AuthenticationType { get;}

    } // interface IAuthenticationModule


} // namespace System.Net
