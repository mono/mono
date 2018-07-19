//------------------------------------------------------------------------------
// <copyright file="AuthenticationManagerBase.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Globalization;
    using System.Net.Configuration;
    using System.Reflection;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Permissions;
    using System;
    using System.Threading;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    internal abstract class AuthenticationManagerBase : IAuthenticationManager
    {
        private static volatile ICredentialPolicy s_ICredentialPolicy;
        private static SpnDictionary m_SpnDictionary = new SpnDictionary();

        private static TriState s_OSSupportsExtendedProtection = TriState.Unspecified;
        private static TriState s_SspSupportsExtendedProtection = TriState.Unspecified;

        public ICredentialPolicy CredentialPolicy
        {
            get
            {
                return s_ICredentialPolicy;
            }
            set
            {
                s_ICredentialPolicy = value;
            }
        }

        public virtual void EnsureConfigLoaded()
        {
            // No-op: performed at object creation.
        }

        public StringDictionary CustomTargetNameDictionary
        {
            get { return m_SpnDictionary; }
        }
        //
        // This will give access to some internal methods
        //
        public SpnDictionary SpnDictionary
        {
            get { return m_SpnDictionary; }
        }

        [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "Reviewed for thread-safety.")]
        public bool OSSupportsExtendedProtection
        {

            get
            {

                if (s_OSSupportsExtendedProtection == TriState.Unspecified)
                {
                    if (ComNetOS.IsWin7orLater)
                    {
                        s_OSSupportsExtendedProtection = TriState.True;
                    }
                    else
                    {
                        if (SspSupportsExtendedProtection)
                        {
                            // EP is considered supported only if both SSPs and http.sys support CBT/EP. 
                            // We don't support scenarios where e.g. only SSPs support CBT. In such cases 
                            // the customer needs to patch also http.sys (even if he may not use it).
                            if (UnsafeNclNativeMethods.HttpApi.ExtendedProtectionSupported)
                            {
                                s_OSSupportsExtendedProtection = TriState.True;
                            }
                            else
                            {
                                s_OSSupportsExtendedProtection = TriState.False;
                            }
                        }
                        else
                        {
                            s_OSSupportsExtendedProtection = TriState.False;
                        }
                    }
                }

                return (s_OSSupportsExtendedProtection == TriState.True);
            }
        }

        [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "Reviewed for thread-safety.")]
        public bool SspSupportsExtendedProtection
        {

            get
            {

                if (s_SspSupportsExtendedProtection == TriState.Unspecified)
                {
                    if (ComNetOS.IsWin7orLater)
                    {
                        s_SspSupportsExtendedProtection = TriState.True;
                    }
                    else
                    {
                        // Perform a loopback NTLM authentication to determine whether the underlying OS supports 
                        // extended protection
                        ContextFlags clientFlags = ContextFlags.Connection | ContextFlags.InitIdentify;

                        NTAuthentication client = new NTAuthentication(false, NtlmClient.AuthType,
                            SystemNetworkCredential.defaultCredential, "http/localhost", clientFlags, null);
                        try
                        {

                            NTAuthentication server = new NTAuthentication(true, NtlmClient.AuthType,
                                SystemNetworkCredential.defaultCredential, null, ContextFlags.Connection, null);
                            try
                            {

                                SecurityStatus status;
                                byte[] blob = null;

                                while (!server.IsCompleted)
                                {
                                    blob = client.GetOutgoingBlob(blob, true, out status);
                                    blob = server.GetOutgoingBlob(blob, true, out status);
                                }

                                if (server.OSSupportsExtendedProtection)
                                {
                                    s_SspSupportsExtendedProtection = TriState.True;
                                }
                                else
                                {
                                    if (Logging.On) Logging.PrintWarning(Logging.Web, SR.GetString(SR.net_ssp_dont_support_cbt));
                                    s_SspSupportsExtendedProtection = TriState.False;
                                }
                            }
                            finally
                            {
                                server.CloseContext();
                            }
                        }
                        finally
                        {
                            client.CloseContext();
                        }
                    }
                }

                return (s_SspSupportsExtendedProtection == TriState.True);
            }
        }

        /// <devdoc>
        ///    <para>Call each registered authentication module to determine the first module that
        ///       can respond to the authentication request.</para>
        /// </devdoc>
        public abstract Authorization Authenticate(string challenge, WebRequest request, ICredentials credentials);

        // These four authentication modules require a Channel Binding Token to be able to preauthenticate over https.
        // After a successful authentication, they will cache the CBT used on the ServicePoint.  In order to PreAuthenticate,
        // they require that a CBT has previously been cached.  Any other module should be allowed to try preauthentication
        // without a cached CBT
#if DEBUG
        // This method is only called as part of an assert
        protected static bool ModuleRequiresChannelBinding(IAuthenticationModule authenticationModule)
        {
            return (authenticationModule is NtlmClient || authenticationModule is KerberosClient ||
                    authenticationModule is NegotiateClient || authenticationModule is DigestClient);
        }
#endif

        /// <devdoc>
        ///    <para>Pre-authenticates a request.</para>
        /// </devdoc>
        public abstract Authorization PreAuthenticate(WebRequest request, ICredentials credentials);

        /// <devdoc>
        ///    <para>Registers an authentication module with the authentication manager.</para>
        /// </devdoc>
        public abstract void Register(IAuthenticationModule authenticationModule);

        /// <devdoc>
        ///    <para>Unregisters authentication modules for an authentication scheme.</para>
        /// </devdoc>
        public abstract void Unregister(IAuthenticationModule authenticationModule);
        /// <devdoc>
        ///    <para>Unregisters authentication modules for an authentication scheme.</para>
        /// </devdoc>
        public abstract void Unregister(string authenticationScheme);

        /// <devdoc>
        ///    <para>
        ///       Returns a list of registered authentication modules.
        ///    </para>
        /// </devdoc>
        public abstract IEnumerator RegisteredModules
        {
            get;
        }

        /// <devdoc>
        ///    <para>
        ///       Binds an authentication response to a request for pre-authentication.
        ///    </para>
        /// </devdoc>
        // Create binding between an authorization response and the module
        // generating that response
        // This association is used for deciding which module to invoke
        // for preauthentication purposes
        public abstract void BindModule(Uri uri, Authorization response, IAuthenticationModule module);

        // This function returns a prefix of the given absolute Uri
        // which will be used for associating authentication information
        // The purpose is to associate the module-binding not with a single
        // Uri but some collection generalizing that Uri to the loosely-defined
        // notion of "protection realm"
        protected static string generalize(Uri location)
        {
            string completeUri = location.GetComponents(UriComponents.AbsoluteUri
                & ~(UriComponents.Query | UriComponents.Fragment), UriFormat.UriEscaped);
            int lastFwdSlash = completeUri.LastIndexOf('/');
            if (lastFwdSlash < 0)
            {
                return completeUri;
            }
            return completeUri.Substring(0, lastFwdSlash + 1);
        }
    }; // class AuthenticationManagerBase

} // namespace System.Net
