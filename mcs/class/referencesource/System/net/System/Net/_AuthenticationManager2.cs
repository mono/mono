//------------------------------------------------------------------------------
// <copyright file="_AuthenticationManager2.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Net.Configuration;
    using System.Reflection;
    using System.Security.Authentication.ExtendedProtection;

    internal class AuthenticationManager2 : AuthenticationManagerBase
    {
        private PrefixLookup moduleBinding = null;
        private ConcurrentDictionary<string, IAuthenticationModule> moduleList = null;
        
        public AuthenticationManager2()
        {
            this.moduleBinding = new PrefixLookup();
            InitializeModuleList();
        }

        public AuthenticationManager2(int maxPrefixLookupEntries)
        {
            this.moduleBinding = new PrefixLookup(maxPrefixLookupEntries);
            InitializeModuleList();
        }

        /// <devdoc>
        ///    <para>Call each registered authentication module to determine the first module that
        ///       can respond to the authentication request.</para>
        /// </devdoc>
        public override Authorization Authenticate(string challenge, WebRequest request, ICredentials credentials)
        {
            //
            // parameter validation
            //
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (credentials == null)
            {
                throw new ArgumentNullException("credentials");
            }

            if (challenge == null)
            {
                throw new ArgumentNullException("challenge");
            }

            GlobalLog.Print("AuthenticationManager::Authenticate() challenge:[" + challenge + "]");

            Authorization response = null;

            HttpWebRequest httpWebRequest = request as HttpWebRequest;
            if (httpWebRequest != null && httpWebRequest.CurrentAuthenticationState.Module != null)
            {
                response = 
                    httpWebRequest.CurrentAuthenticationState.Module.Authenticate(challenge, request, credentials);
            }
            else
            {
                // This is the case where we would try to find the module on the first server challenge
                foreach (IAuthenticationModule authenticationModule in this.moduleList.Values)
                {
                    //
                    // the AuthenticationModule will
                    // 1) return a valid string on success
                    // 2) return null if it knows it cannot respond
                    // 3) throw if it could have responded but unexpectedly failed to do so
                    //
                    if (httpWebRequest != null)
                    {
                        httpWebRequest.CurrentAuthenticationState.Module = authenticationModule;
                    }

                    response = authenticationModule.Authenticate(challenge, request, credentials);

                    if (response != null)
                    {
                        //
                        // found the Authentication Module, return it
                        //
                        GlobalLog.Print("AuthenticationManager::Authenticate() found IAuthenticationModule:[" 
                                        + authenticationModule.AuthenticationType + "]");
                        break;
                    }
                }
            }

            return response;
        }

        /// <devdoc>
        ///    <para>Pre-authenticates a request.</para>
        /// </devdoc>
        public override Authorization PreAuthenticate(WebRequest request, ICredentials credentials)
        {
            GlobalLog.Print("AuthenticationManager::PreAuthenticate() request:" 
                + ValidationHelper.HashString(request) + " credentials:" + ValidationHelper.HashString(credentials));

            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (credentials == null)
            {
                return null;
            }

            HttpWebRequest httpWebRequest = request as HttpWebRequest;
            IAuthenticationModule authenticationModule;
            if (httpWebRequest == null)
            {
                return null;
            }

            //
            // PrefixLookup is thread-safe
            //
            string moduleName = moduleBinding.Lookup(httpWebRequest.ChallengedUri.AbsoluteUri) as string;
            GlobalLog.Print("AuthenticationManager::PreAuthenticate() s_ModuleBinding.Lookup returns:" 
                            + ValidationHelper.ToString(moduleName));

            if (moduleName == null)
            {
                return null;
            }

            if (!this.moduleList.TryGetValue(moduleName.ToUpperInvariant(), out authenticationModule))
            {
                // The module could have been unregistered
                // No preauthentication is possible
                return null;
            }

            // prepopulate the channel binding token so we can try preauth (but only for modules that actually need it!)
            if (httpWebRequest.ChallengedUri.Scheme == Uri.UriSchemeHttps)
            {
                object binding = httpWebRequest.ServicePoint.CachedChannelBinding;

#if DEBUG
                // the ModuleRequiresChannelBinding method is only compiled in DEBUG so the assert must be restricted to DEBUG
                // as well

                // If the authentication module does CBT, we require that it also caches channel bindings.
                System.Diagnostics.Debug.Assert(!(binding == null && ModuleRequiresChannelBinding(authenticationModule)));
#endif

                // can also be DBNull.Value, indicating "we previously succeeded without getting a CBT."
                // (ie, unpatched SSP talking to a partially-hardened server)
                ChannelBinding channelBinding = binding as ChannelBinding;
                if (channelBinding != null)
                {
                    httpWebRequest.CurrentAuthenticationState.TransportContext = 
                        new CachedTransportContext(channelBinding);
                }
            }

            // Otherwise invoke the PreAuthenticate method
            // we're guaranteed that CanPreAuthenticate is true because we check before calling BindModule()
            Authorization authorization = authenticationModule.PreAuthenticate(request, credentials);

            if (authorization != null && !authorization.Complete && httpWebRequest != null)
            {
                httpWebRequest.CurrentAuthenticationState.Module = authenticationModule;
            }

            GlobalLog.Print(
                "AuthenticationManager::PreAuthenticate() IAuthenticationModule.PreAuthenticate()" 
                + " returned authorization:" + ValidationHelper.HashString(authorization));

            return authorization;
        }

        /// <devdoc>
        ///    <para>Registers an authentication module with the authentication manager.</para>
        /// </devdoc>
        public override void Register(IAuthenticationModule authenticationModule)
        {
            if (authenticationModule == null)
            {
                throw new ArgumentNullException("authenticationModule");
            }

            GlobalLog.Print(
                "AuthenticationManager::Register() registering :[" + authenticationModule.AuthenticationType + "]");

            string normalizedAuthenticationType = authenticationModule.AuthenticationType.ToUpperInvariant();

            this.moduleList.AddOrUpdate(
                normalizedAuthenticationType,
                authenticationModule,
                (key, value) => authenticationModule);
        }

        /// <devdoc>
        ///    <para>Unregisters authentication modules for an authentication scheme.</para>
        /// </devdoc>
        public override void Unregister(IAuthenticationModule authenticationModule)
        {
            if (authenticationModule == null)
            {
                throw new ArgumentNullException("authenticationModule");
            }

            GlobalLog.Print(
                "AuthenticationManager::Unregister() unregistering :[" 
                + authenticationModule.AuthenticationType + "]");

            string normalizedAuthenticationType = authenticationModule.AuthenticationType.ToUpperInvariant();
            UnregisterInternal(normalizedAuthenticationType);
        }

        /// <devdoc>
        ///    <para>Unregisters authentication modules for an authentication scheme.</para>
        /// </devdoc>
        public override void Unregister(string authenticationScheme)
        {
            if (authenticationScheme == null)
            {
                throw new ArgumentNullException("authenticationScheme");
            }

            GlobalLog.Print("AuthenticationManager::Unregister() unregistering :[" + authenticationScheme + "]");

            string normalizedAuthenticationType = authenticationScheme.ToUpperInvariant();
            UnregisterInternal(normalizedAuthenticationType);
        }

        /// <devdoc>
        ///    <para>
        ///       Returns a list of registered authentication modules.
        ///    </para>
        /// </devdoc>
        public override IEnumerator RegisteredModules
        {
            get
            {
                return this.moduleList.Values.GetEnumerator();
            }
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
        public override void BindModule(Uri uri, Authorization response, IAuthenticationModule module)
        {
            GlobalLog.Assert(
                module.CanPreAuthenticate, 
                "AuthenticationManager::BindModule()|module.CanPreAuthenticate == false");

            if (response.ProtectionRealm != null)
            {
                // The authentication module specified which Uri prefixes
                // will be preauthenticated
                string[] prefix = response.ProtectionRealm;

                for (int k = 0; k < prefix.Length; k++)
                {
                    //
                    // PrefixLookup is thread-safe
                    //
                    moduleBinding.Add(prefix[k], module.AuthenticationType.ToUpperInvariant());
                }
            }
            else
            {
                // Otherwise use the default policy for "fabricating"
                // some protection realm generalizing the particular Uri
                string prefix = generalize(uri);
                //
                // PrefixLookup is thread-safe
                //
                moduleBinding.Add(prefix, module.AuthenticationType);
            }
        }
        
        [SuppressMessage(
            "Microsoft.Design", 
            "CA1031", 
            Justification = "Previous behavior of AuthenticationManager is to catch all exceptions thrown by all " + 
            "IAuthenticationModule plugins.")]
#if !TRAVE
        [SuppressMessage(
            "Microsoft.Performance",
            "CA1804",
            Justification = "Variable exception is used for logging purposes.")]
#endif
        private void InitializeModuleList()
        {
            GlobalLog.Print(
                "AuthenticationManager::Initialize(): calling ConfigurationManager.GetSection()");

            // This will never come back as null. Additionally, it will
            // have the items the user wants available.
            List<Type> authenticationModuleTypes =
                AuthenticationModulesSectionInternal.GetSection().AuthenticationModules;

            //
            // Should be registered in a growing list of encryption/algorithm strengths
            //  basically, walk through a list of Types, and create new Auth objects
            //  from them.
            //
            // order is meaningful here:
            // load the registered list of auth types
            // with growing level of encryption.
            //

            this.moduleList = new ConcurrentDictionary<string, IAuthenticationModule>();
            IAuthenticationModule moduleToRegister;
            foreach (Type type in authenticationModuleTypes)
            {
                try
                {
                    moduleToRegister = Activator.CreateInstance(type,
                                        BindingFlags.CreateInstance
                                        | BindingFlags.Instance
                                        | BindingFlags.NonPublic
                                        | BindingFlags.Public,
                                        null,          // Binder
                                        new object[0], // no arguments
                                        CultureInfo.InvariantCulture
                                        ) as IAuthenticationModule;
                    if (moduleToRegister != null)
                    {
                        GlobalLog.Print(
                            "WebRequest::Initialize(): Register:" + moduleToRegister.AuthenticationType);

                        string normalizedAuthenticationType =
                            moduleToRegister.AuthenticationType.ToUpperInvariant();

                        this.moduleList.AddOrUpdate(
                            normalizedAuthenticationType,
                            moduleToRegister,
                            (key, value) => moduleToRegister);
                    }
                }
                catch (Exception exception)
                {
                    //
                    // ignore failure (log exception for debugging)
                    //
                    GlobalLog.Print(
                        "AuthenticationManager::constructor failed to initialize: " 
                        + exception.ToString());
                }
            }
        }

        private void UnregisterInternal(string normalizedAuthenticationType)
        {
            IAuthenticationModule removedModule;
            if (!this.moduleList.TryRemove(normalizedAuthenticationType, out removedModule))
            {
                throw new InvalidOperationException(SR.GetString(SR.net_authmodulenotregistered));
            }
        }
    } // class AuthenticationManager2
} // namespace System.Net
