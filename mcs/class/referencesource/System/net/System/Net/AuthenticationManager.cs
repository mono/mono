//------------------------------------------------------------------------------
// <copyright file="AuthenticationManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {

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


    //
    // A contract that applications can use to restrict auth scenarios in current appDomain
    //
    public interface ICredentialPolicy
    {
        bool ShouldSendCredential(Uri challengeUri, WebRequest request, NetworkCredential credential, IAuthenticationModule authenticationModule);
    }

    /// <devdoc>
    ///    <para>Manages the authentication modules called during the client authentication
    ///       process.</para>
    /// </devdoc>
    public class AuthenticationManager {

        //also used as a lock object
        private static PrefixLookup s_ModuleBinding = new PrefixLookup();

        private static volatile ArrayList s_ModuleList;
        private static volatile ICredentialPolicy s_ICredentialPolicy;
        private static SpnDictionary m_SpnDictionary = new SpnDictionary();

        private static TriState s_OSSupportsExtendedProtection = TriState.Unspecified;
        private static TriState s_SspSupportsExtendedProtection = TriState.Unspecified;

        // not creatable...
        //
        private AuthenticationManager() {
        }

        //
        //
        //
        public static ICredentialPolicy CredentialPolicy {
            get {
                return s_ICredentialPolicy;
            }
            set {
                ExceptionHelper.ControlPolicyPermission.Demand();
                s_ICredentialPolicy = value;
            }
        }
        //
        //
        public static StringDictionary CustomTargetNameDictionary {
            get {return m_SpnDictionary;}
        }
        //
        // This will give access to some internal methods
        //
        internal static SpnDictionary SpnDictionary {
            get {return m_SpnDictionary;}
        }

        //
        //
        internal static void EnsureConfigLoaded() {
            try {
                object o = ModuleList;
            }
            catch (Exception e) {
                if (e is ThreadAbortException || e is OutOfMemoryException || e is StackOverflowException)
                    throw;
                // A Config System has circular dependency on HttpWebRequest so they call this method to
                // trigger the config. For some reason they don't want any exceptions from here.
            }           
        }

        [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "Reviewed for thread-safety.")]
        internal static bool OSSupportsExtendedProtection {

            get {

                if (s_OSSupportsExtendedProtection == TriState.Unspecified) {
                    if (ComNetOS.IsWin7orLater) {
                        s_OSSupportsExtendedProtection = TriState.True;
                    }
                    else {
                        if (SspSupportsExtendedProtection) {
                            // EP is considered supported only if both SSPs and http.sys support CBT/EP. 
                            // We don't support scenarios where e.g. only SSPs support CBT. In such cases 
                            // the customer needs to patch also http.sys (even if he may not use it).
                            if (UnsafeNclNativeMethods.HttpApi.ExtendedProtectionSupported) {
                                s_OSSupportsExtendedProtection = TriState.True;
                            }
                            else {
                                s_OSSupportsExtendedProtection = TriState.False;
                            }
                        }
                        else {
                            s_OSSupportsExtendedProtection = TriState.False;
                        }
                    }
                }

                return (s_OSSupportsExtendedProtection == TriState.True);
            }
        }

        [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "Reviewed for thread-safety.")]
        internal static bool SspSupportsExtendedProtection {

            get {

                if (s_SspSupportsExtendedProtection == TriState.Unspecified) { 
                    if (ComNetOS.IsWin7orLater) {
                        s_SspSupportsExtendedProtection = TriState.True;
                    }
                    else {
                        // Perform a loopback NTLM authentication to determine whether the underlying OS supports 
                        // extended protection
                        ContextFlags clientFlags = ContextFlags.Connection | ContextFlags.InitIdentify;

                        NTAuthentication client = new NTAuthentication(false, NtlmClient.AuthType, 
                            SystemNetworkCredential.defaultCredential, "http/localhost", clientFlags, null);
                        try {

                            NTAuthentication server = new NTAuthentication(true, NtlmClient.AuthType, 
                                SystemNetworkCredential.defaultCredential, null, ContextFlags.Connection, null);
                            try {

                                SecurityStatus status;
                                byte[] blob = null;

                                while (!server.IsCompleted) {
                                    blob = client.GetOutgoingBlob(blob, true, out status);
                                    blob = server.GetOutgoingBlob(blob, true, out status);
                                }

                                if (server.OSSupportsExtendedProtection) {
                                    s_SspSupportsExtendedProtection = TriState.True;
                                }
                                else {
                                    if (Logging.On) Logging.PrintWarning(Logging.Web, SR.GetString(SR.net_ssp_dont_support_cbt));
                                    s_SspSupportsExtendedProtection = TriState.False;
                                }
                            }
                            finally {
                                server.CloseContext();
                            }
                        }
                        finally {
                            client.CloseContext();
                        }
                    }
                }

                return (s_SspSupportsExtendedProtection == TriState.True);
            }
        }

        //
        // ModuleList - static initialized property -
        //  contains list of Modules used for Authentication
        //

        private static ArrayList ModuleList {

            get {

                //
                // GetConfig() might use us, so we have a circular dependency issue,
                // that causes us to nest here, we grab the lock, only
                // if we haven't initialized, or another thread is busy in initialization
                //

                if (s_ModuleList == null) {
                    lock (s_ModuleBinding) {
                        if (s_ModuleList == null) {
                            GlobalLog.Print("AuthenticationManager::Initialize(): calling ConfigurationManager.GetSection()");

                            // This will never come back as null. Additionally, it will
                            // have the items the user wants available.
                            List<Type> authenticationModuleTypes =  AuthenticationModulesSectionInternal.GetSection().AuthenticationModules;

                            //
                            // Should be registered in a growing list of encryption/algorithm strengths
                            //  basically, walk through a list of Types, and create new Auth objects
                            //  from them.
                            //
                            // order is meaningful here:
                            // load the registered list of auth types
                            // with growing level of encryption.
                            //

                            ArrayList moduleList = new ArrayList();
                            IAuthenticationModule moduleToRegister;
                            foreach (Type type in authenticationModuleTypes){
                                try {
                                    moduleToRegister = Activator.CreateInstance(type,
                                                        BindingFlags.CreateInstance
                                                        | BindingFlags.Instance
                                                        | BindingFlags.NonPublic
                                                        | BindingFlags.Public,
                                                        null,          // Binder
                                                        new object[0], // no arguments
                                                        CultureInfo.InvariantCulture
                                                        ) as IAuthenticationModule;
                                    if (moduleToRegister != null) {
                                        GlobalLog.Print("WebRequest::Initialize(): Register:" + moduleToRegister.AuthenticationType);
                                        RemoveAuthenticationType(moduleList, moduleToRegister.AuthenticationType);
                                        moduleList.Add(moduleToRegister);
                                    }
                                }
                                catch (Exception exception) {
                                    //
                                    // ignore failure (log exception for debugging)
                                    //
                                    GlobalLog.Print("AuthenticationManager::constructor failed to initialize: " + exception.ToString());
                                }                                
                            }

                            s_ModuleList = moduleList;
                        }
                    }
                }

                return s_ModuleList;
            }
        }


        private static void RemoveAuthenticationType(ArrayList list, string typeToRemove) {
            for (int i=0; i< list.Count; ++i) {
                if (string.Compare(((IAuthenticationModule)list[i]).AuthenticationType, typeToRemove, StringComparison.OrdinalIgnoreCase) ==0) {
                    list.RemoveAt(i);
                    break;
                }

            }
        }

        /// <devdoc>
        ///    <para>Call each registered authentication module to determine the first module that
        ///       can respond to the authentication request.</para>
        /// </devdoc>
        public static Authorization Authenticate(string challenge, WebRequest request, ICredentials credentials) {
            //
            // parameter validation
            //
            if (request == null) {
                throw new ArgumentNullException("request");
            }
            if (credentials == null) {
                throw new ArgumentNullException("credentials");
            }
            if (challenge==null) {
                throw new ArgumentNullException("challenge");
            }

            GlobalLog.Print("AuthenticationManager::Authenticate() challenge:[" + challenge + "]");

            Authorization response = null;

            HttpWebRequest httpWebRequest = request as HttpWebRequest;
            if (httpWebRequest != null && httpWebRequest.CurrentAuthenticationState.Module != null)
            {
                response = httpWebRequest.CurrentAuthenticationState.Module.Authenticate(challenge, request, credentials);
            }
            else
            {
                // This is the case where we would try to find the module on the first server challenge
                lock (s_ModuleBinding) {
                    //
                    // fastest way of iterating on the ArryList
                    //
                    for (int i = 0; i < ModuleList.Count; i++) {
                        IAuthenticationModule authenticationModule = (IAuthenticationModule)ModuleList[i];
                        //
                        // the AuthenticationModule will
                        // 1) return a valid string on success
                        // 2) return null if it knows it cannot respond
                        // 3) throw if it could have responded but unexpectedly failed to do so
                        //
                        if (httpWebRequest != null) {
                            httpWebRequest.CurrentAuthenticationState.Module = authenticationModule;
                        }
                        response = authenticationModule.Authenticate(challenge, request, credentials);

                        if (response!=null) {
                            //
                            // found the Authentication Module, return it
                            //
                            GlobalLog.Print("AuthenticationManager::Authenticate() found IAuthenticationModule:[" + authenticationModule.AuthenticationType + "]");
                            break;
                        }
                    }
                }
            }

            return response;
        }

        // These four authentication modules require a Channel Binding Token to be able to preauthenticate over https.
        // After a successful authentication, they will cache the CBT used on the ServicePoint.  In order to PreAuthenticate,
        // they require that a CBT has previously been cached.  Any other module should be allowed to try preauthentication
        // without a cached CBT
#if DEBUG
        // This method is only called as part of an assert
        private static bool ModuleRequiresChannelBinding(IAuthenticationModule authenticationModule)
        {
            return (authenticationModule is NtlmClient || authenticationModule is KerberosClient ||
                    authenticationModule is NegotiateClient || authenticationModule is DigestClient);
        }
#endif

        /// <devdoc>
        ///    <para>Pre-authenticates a request.</para>
        /// </devdoc>
        public static Authorization PreAuthenticate(WebRequest request, ICredentials credentials) {
            GlobalLog.Print("AuthenticationManager::PreAuthenticate() request:" + ValidationHelper.HashString(request) + " credentials:" + ValidationHelper.HashString(credentials));
            if (request == null) {
                throw new ArgumentNullException("request");
            }
            if (credentials == null) {
                return null;
            }

            HttpWebRequest httpWebRequest = request as HttpWebRequest;
            IAuthenticationModule authenticationModule;
            if (httpWebRequest == null)
                return null;

            //
            // PrefixLookup is thread-safe
            //
            string moduleName = s_ModuleBinding.Lookup(httpWebRequest.ChallengedUri.AbsoluteUri) as string;
            GlobalLog.Print("AuthenticationManager::PreAuthenticate() s_ModuleBinding.Lookup returns:" + ValidationHelper.ToString(moduleName));
            if (moduleName == null)
                return null;
            authenticationModule = findModule(moduleName);
            if (authenticationModule == null)
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
                    httpWebRequest.CurrentAuthenticationState.TransportContext = new CachedTransportContext(channelBinding);
                }
            }

            // Otherwise invoke the PreAuthenticate method
            // we're guaranteed that CanPreAuthenticate is true because we check before calling BindModule()
            Authorization authorization = authenticationModule.PreAuthenticate(request, credentials);

            if (authorization != null && !authorization.Complete && httpWebRequest != null)
                httpWebRequest.CurrentAuthenticationState.Module = authenticationModule;

            GlobalLog.Print("AuthenticationManager::PreAuthenticate() IAuthenticationModule.PreAuthenticate() returned authorization:" + ValidationHelper.HashString(authorization));
            return authorization;
        }


        /// <devdoc>
        ///    <para>Registers an authentication module with the authentication manager.</para>
        /// </devdoc>
        public static void Register(IAuthenticationModule authenticationModule) {
            ExceptionHelper.UnmanagedPermission.Demand();
            if (authenticationModule == null) {
                throw new ArgumentNullException("authenticationModule");
            }
            GlobalLog.Print("AuthenticationManager::Register() registering :[" + authenticationModule.AuthenticationType + "]");
            lock (s_ModuleBinding) {
                IAuthenticationModule existentModule = findModule(authenticationModule.AuthenticationType);
                if (existentModule != null) {
                    ModuleList.Remove(existentModule);
                }
                ModuleList.Add(authenticationModule);
            }
        }

        /// <devdoc>
        ///    <para>Unregisters authentication modules for an authentication scheme.</para>
        /// </devdoc>
        public static void Unregister(IAuthenticationModule authenticationModule) {
            ExceptionHelper.UnmanagedPermission.Demand();
            if (authenticationModule == null) {
                throw new ArgumentNullException("authenticationModule");
            }
            GlobalLog.Print("AuthenticationManager::Unregister() unregistering :[" + authenticationModule.AuthenticationType + "]");
            lock (s_ModuleBinding) {
                if (!ModuleList.Contains(authenticationModule)) {
                    throw new InvalidOperationException(SR.GetString(SR.net_authmodulenotregistered));
                }
                ModuleList.Remove(authenticationModule);
            }
        }
        /// <devdoc>
        ///    <para>Unregisters authentication modules for an authentication scheme.</para>
        /// </devdoc>
        public static void Unregister(string authenticationScheme) {
            ExceptionHelper.UnmanagedPermission.Demand();
            if (authenticationScheme == null) {
                throw new ArgumentNullException("authenticationScheme");
            }
            GlobalLog.Print("AuthenticationManager::Unregister() unregistering :[" + authenticationScheme + "]");
            lock (s_ModuleBinding) {
                IAuthenticationModule existentModule = findModule(authenticationScheme);
                if (existentModule == null) {
                    throw new InvalidOperationException(SR.GetString(SR.net_authschemenotregistered));
                }
                ModuleList.Remove(existentModule);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Returns a list of registered authentication modules.
        ///    </para>
        /// </devdoc>
        public static IEnumerator RegisteredModules {
            get {
                return ModuleList.GetEnumerator();
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
        internal static void BindModule(Uri uri, Authorization response, IAuthenticationModule module) {
            GlobalLog.Assert(module.CanPreAuthenticate, "AuthenticationManager::BindModule()|module.CanPreAuthenticate == false");
            if (response.ProtectionRealm!=null) {
                // The authentication module specified which Uri prefixes
                // will be preauthenticated
                string[] prefix = response.ProtectionRealm;

                for (int k=0; k<prefix.Length; k++) {
                    //
                    // PrefixLookup is thread-safe
                    //
                    s_ModuleBinding.Add(prefix[k], module.AuthenticationType);
                }
            }
            else {
                // Otherwise use the default policy for "fabricating"
                // some protection realm generalizing the particular Uri
                string prefix = generalize(uri);
                //
                // PrefixLookup is thread-safe
                //
                s_ModuleBinding.Add(prefix, module.AuthenticationType);
            }
        }

        //
        // Lookup module by AuthenticationType
        //
        private static IAuthenticationModule findModule(string authenticationType) {
            IAuthenticationModule returnAuthenticationModule = null;
            ArrayList moduleList = ModuleList;
            IAuthenticationModule authenticationModule;
            for (int k=0; k<moduleList.Count; k++) {
                authenticationModule = (IAuthenticationModule)moduleList[k];
                if (string.Compare(authenticationModule.AuthenticationType, authenticationType, StringComparison.OrdinalIgnoreCase) == 0) {
                    returnAuthenticationModule = authenticationModule;
                    break;
                }
            }
            return returnAuthenticationModule;
        }

        // This function returns a prefix of the given absolute Uri
        // which will be used for associating authentication information
        // The purpose is to associate the module-binding not with a single
        // Uri but some collection generalizing that Uri to the loosely-defined
        // notion of "protection realm"
        private static string generalize(Uri location) {
            string completeUri = location.GetComponents(UriComponents.AbsoluteUri 
                & ~(UriComponents.Query | UriComponents.Fragment), UriFormat.UriEscaped);
            int lastFwdSlash = completeUri.LastIndexOf('/');
            if (lastFwdSlash < 0) {
                return completeUri;
            }
            return completeUri.Substring(0, lastFwdSlash+1);
        }

        //
        // The method will extract the blob that does correspond to the moduled with the name passed in signature parameter
        // The method avoids confusion arisen from the parameters passed in a quoted string, such as:
        // WWW-Authenticate: Digest username="NTLM", realm="wit", NTLM ...
        //
        internal static int FindSubstringNotInQuotes(string challenge, string signature) {
            int index = -1;
            Debug.Assert(signature.ToLowerInvariant().Equals(signature,StringComparison.InvariantCulture), 
                "'signature' parameter must be lower case");
            if (challenge != null && signature != null && challenge.Length>=signature.Length) {
                int firstQuote = -1, secondQuote = -1;
                for (int i = 0; i < challenge.Length && index < 0; i++)
                {
                    // Search for the quotes
                    if (challenge[i]=='\"')
                    {
                        if (firstQuote <= secondQuote)
                            firstQuote = i;
                        else
                            secondQuote = i;
                    }
                    // We've found both ends of an unquoted segment (could be whole challenge), search inside for the signature.
                    if (i==challenge.Length-1 || (challenge[i]=='\"' && firstQuote>secondQuote))
                    {
                        // see if the portion of challenge out of the quotes contains
                        // the signature of the IAuthenticationModule
                        if (i==challenge.Length-1)
                            firstQuote = challenge.Length;
                        // unquoted segment is too small to hold a scheme name, ie: scheme param="value",a=""
                        if (firstQuote<secondQuote + 3)
                            continue;

                        int checkstart = secondQuote + 1;
                        int checkLength = firstQuote - secondQuote - 1;
                        do
                        {
                            // Search for the next (partial match) occurance of the signature
                            index = IndexOf(challenge, signature, checkstart, checkLength);

                            if (index >= 0)
                            {
                                // Verify the signature is a full scheme name match, not a partial match or a parameter name:
                                if ((index == 0 || challenge[index - 1] == ' ' || challenge[index - 1] == ',') &&
                                    (index + signature.Length == challenge.Length || challenge[index + signature.Length] == ' ' || challenge[index + signature.Length] == ','))
                                {
                                    break;
                                }
                                // Only a partial match / param name, but maybe there is another occurance of the signature later?
                                checkLength -= index - checkstart + 1;
                                checkstart = index + 1;
                            }
                        } while (index >= 0);
                    }
                }
            }
            GlobalLog.Print("AuthenticationManager::FindSubstringNotInQuotes(" + challenge + ", " + signature + ")=" + index.ToString());
            return index;
        }
        //
        // Helper for FindSubstringNotInQuotes
        // Find the FIRST possible index of a signature.
        private static int IndexOf(string challenge, string lwrCaseSignature, int start, int count)
        {
            count += start + 1 - lwrCaseSignature.Length;
            for (; start < count; ++start)
            {
                int i = 0;
                for (; i < lwrCaseSignature.Length; ++i)
                {
                    // force a challenge char to lowecase (safe assuming it works on trusted ASCII source)
                    if ((challenge[start+i] | 0x20) != lwrCaseSignature[i])
                        break;
                }
                if (i == lwrCaseSignature.Length)
                    return start;
            }
            return -1;
        }
        //
        // this method is called by the IAuthenticationModule implementations
        // (mainly Digest) to safely find their list of parameters in a challenge.
        // it returns the index of the first ',' that is not included in quotes,
        // -1 is returned on error or end of string. on return offset contains the
        // index of the first '=' that is not included in quotes, -1 if no '=' was found.
        //
        internal static int SplitNoQuotes(string challenge, ref int offset) {
            // GlobalLog.Print("SplitNoQuotes([" + challenge + "], " + offset.ToString() + ")");
            //
            // save offset
            //
            int realOffset = offset;
            //
            // default is not found
            //
            offset = -1;

            if (challenge != null && realOffset<challenge.Length) {
                int firstQuote = -1, secondQuote = -1;

                for (int i = realOffset; i < challenge.Length; i++) {
                    //
                    // firstQuote>secondQuote means we are in a quoted string
                    //
                    if (firstQuote>secondQuote && challenge[i]=='\\' && i+1 < challenge.Length && challenge[i+1]=='\"') {
                        //
                        // skip <\"> when in a quoted string
                        //
                        i++;
                    }
                    else if (challenge[i]=='\"') {
                        if (firstQuote <= secondQuote) {
                            firstQuote = i;
                        }
                        else {
                            secondQuote = i;
                        }
                    }
                    else if (challenge[i]=='=' && firstQuote<=secondQuote && offset<0) {
                        offset = i;
                    }
                    else if (challenge[i]==',' && firstQuote<=secondQuote) {
                        return i;
                    }
                }
            }

            return -1;
        }

#if !FEATURE_PAL
        internal static Authorization GetGroupAuthorization(IAuthenticationModule thisModule, string token, bool finished, NTAuthentication authSession, bool shareAuthenticatedConnections, bool mutualAuth) {
            return
                new Authorization(
                    token,
                    finished,
                    (shareAuthenticatedConnections) ? null : (thisModule.GetType().FullName + "/" + authSession.UniqueUserId),
                    mutualAuth);

        }
#endif // !FEATURE_PAL

    }; // class AuthenticationManager

    //
    // This internal class implements a data structure which can be
    // used for storing a set of objects keyed by string prefixes
    // Looking up an object given a string returns the value associated
    // with the longest matching prefix
    // (A prefix "matches" a string IFF the string starts with that prefix
    // The degree of the match is prefix length)
    //
    // The class has a configurable maximum capacity.  When adding items, if the
    // list is over capacity, then the least recently used (LRU) item is dropped.
    //
    internal class PrefixLookup {

        // Do not go over this limit.  Discard old data elements
        // Longer lists suffer a search penalty
        private const int defaultCapacity = 100;
        private volatile int capacity = defaultCapacity;
        
        // LRU list - Least Recently Used.  
        // Add new items to the front.  Drop items from the end if beyond capacity.
        // Promote used items to the top.
        private readonly LinkedList<PrefixValuePair> lruList = new LinkedList<PrefixValuePair>();

        private class PrefixValuePair {
            public string prefix;
            public object value;

            public PrefixValuePair(string pre, object val) {
                prefix = pre;
                value = val;
            }
        }

#if DEBUG
        // this method is only called by test code
        internal int Capacity {
            get { return capacity; }
            set {
                lock (lruList) {
                    if (value <= 0) {
                        // Disabled, flush list
                        capacity = 0;
                        lruList.Clear();
                    } else {
                        capacity = value;

                        // Ensure list is still within capacity
                        while (lruList.Count > capacity) {
                            lruList.RemoveLast();
                        }
                    }
                }
            }
        }
#endif

        internal void Add(string prefix, object value) {
            Debug.Assert(prefix != null, "PrefixLookup.Add; prefix must not be null");
            Debug.Assert(prefix.Length > 0, "PrefixLookup.Add; prefix must not be empty");
            Debug.Assert(value != null, "PrefixLookup.Add; value must not be null");

            if (capacity == 0 || prefix == null || prefix.Length == 0 || value == null)
                return;

            // writers are locked
            lock (lruList) {
                // Special case duplicate check at start of list, very common
                if (lruList.First != null && lruList.First.Value.prefix.Equals(prefix)) {
                    // Already in list, update value
                    lruList.First.Value.value = value; 
                } else {
                    // New entry
                    // Duplicates will just be pushed down and eventually discarded
                    lruList.AddFirst(new PrefixValuePair(prefix, value));

                    // If full, drop the least recently used
                    while (lruList.Count > capacity) {
                        lruList.RemoveLast();
                    }
                }

            }
        }

        internal object Lookup(string lookupKey) {
            Debug.Assert(lookupKey != null, "PrefixLookup.Lookup; lookupKey must not be null");
            Debug.Assert(lookupKey.Length > 0, "PrefixLookup.Lookup; lookupKey must not be empty");

            if (lookupKey==null || lookupKey.Length == 0|| lruList.Count == 0) {
                return null;
            }

            LinkedListNode<PrefixValuePair> mostSpecificMatch = null;
            lock (lruList) {
                //
                // Normally readers don't need to be locked, but if the value is found
                // then it is promoted to the top of the list.
                //

                // Oh well, do it the slow way, search for the longest partial match
                string prefix;
                int longestMatchPrefix = 0;
                for (LinkedListNode<PrefixValuePair> pairNode = lruList.First; 
                    pairNode != null; pairNode = pairNode.Next) {
                    //
                    // check if the match is better than the current-most-specific match
                    //
                    prefix = pairNode.Value.prefix;
                    if (prefix.Length > longestMatchPrefix && lookupKey.StartsWith(prefix)) {
                        //
                        // Yes-- update the information about currently preferred match
                        //
                        longestMatchPrefix = prefix.Length;
                        mostSpecificMatch = pairNode;

                        if (longestMatchPrefix == lookupKey.Length)
                            break; // Exact match, optimal solution.
                    }
                }

                if (mostSpecificMatch != null && mostSpecificMatch != lruList.First) {
                    // We have a match and it's not the first element, move it up in the list
                    lruList.Remove(mostSpecificMatch);
                    lruList.AddFirst(mostSpecificMatch);
                }
            }
            return mostSpecificMatch != null ? mostSpecificMatch.Value.value : null;
        }

    } // class PrefixLookup


} // namespace System.Net
