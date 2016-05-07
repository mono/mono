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
        bool ShouldSendCredential(
            Uri challengeUri, 
            WebRequest request, 
            NetworkCredential credential, 
            IAuthenticationModule authenticationModule);
    }

    /// <devdoc>
    ///    <para>Manages the authentication modules called during the client authentication
    ///       process.</para>
    /// </devdoc>
    public class AuthenticationManager 
    {
        private static object instanceLock = new object();
        private static IAuthenticationManager internalInstance = null;
        internal const string authenticationManagerRoot = "System.Net.AuthenticationManager";
        
        // Following names are used both as a per-app key as a global setting
        internal const string configHighPerformance = authenticationManagerRoot + ".HighPerformance";
        internal const string configPrefixLookupMaxCount = authenticationManagerRoot + ".PrefixLookupMaxCount";
        
        private AuthenticationManager()
        {
        }

        private static IAuthenticationManager Instance 
        {
            get
            {
                if (internalInstance == null)
                {
                    lock (instanceLock)
                    {
                        if (internalInstance == null)
                        {
                            internalInstance = SelectAuthenticationManagerInstance();
                        }
                    }
                }

                return internalInstance;
            }
        }

        private static IAuthenticationManager SelectAuthenticationManagerInstance()
        {
            bool highPerformance = false;

            try
            {
                if (RegistryConfiguration.GlobalConfigReadInt(configHighPerformance, 0) == 1)
                {
                    highPerformance = true;
                }
                else if (RegistryConfiguration.AppConfigReadInt(configHighPerformance, 0) == 1)
                {
                    highPerformance = true;
                }
            
                if (highPerformance)
                {
                    int? maxPrefixLookupEntries = ReadPrefixLookupMaxEntriesConfig();
                    if ((maxPrefixLookupEntries != null) && (maxPrefixLookupEntries > 0))
                    {
                        return new AuthenticationManager2((int)maxPrefixLookupEntries);
                    }
                    else
                    {
                        return new AuthenticationManager2();
                    }
                }
            }
            catch (Exception e)
            {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException)
                {
                    throw;
                }
            }
            
            return new AuthenticationManagerDefault();
        }

        private static int? ReadPrefixLookupMaxEntriesConfig()
        {
            int? maxPrefixLookupEntries = null;

            int configuredMaxPrefixLookupEntries =
                RegistryConfiguration.GlobalConfigReadInt(configPrefixLookupMaxCount, -1);

            if (configuredMaxPrefixLookupEntries > 0)
            {
                maxPrefixLookupEntries = configuredMaxPrefixLookupEntries;
            }

            // Per-process setting will override global configuration.
            configuredMaxPrefixLookupEntries =
                RegistryConfiguration.AppConfigReadInt(configPrefixLookupMaxCount, -1);

            if (configuredMaxPrefixLookupEntries > 0)
            {
                maxPrefixLookupEntries = configuredMaxPrefixLookupEntries;
            }
            return maxPrefixLookupEntries;
        }
                
        public static ICredentialPolicy CredentialPolicy {
            get 
            {
                return Instance.CredentialPolicy; 
            }

            set 
            {
                ExceptionHelper.ControlPolicyPermission.Demand();
                Instance.CredentialPolicy = value;
            }
        }

        public static StringDictionary CustomTargetNameDictionary 
        {
            get 
            {
                return Instance.CustomTargetNameDictionary;  
            }
        }

        internal static SpnDictionary SpnDictionary 
        {
            get 
            {
                return Instance.SpnDictionary;
            }
        }

        internal static void EnsureConfigLoaded() 
        {
            Instance.EnsureConfigLoaded();          
        }

        internal static bool OSSupportsExtendedProtection 
        {
            get
            {
                return Instance.OSSupportsExtendedProtection;
            }
        }

        internal static bool SspSupportsExtendedProtection 
        {
            get 
            {
                return Instance.SspSupportsExtendedProtection;
            }
        }

        /// <devdoc>
        ///    <para>Call each registered authentication module to determine the first module that
        ///       can respond to the authentication request.</para>
        /// </devdoc>
        public static Authorization Authenticate(string challenge, WebRequest request, ICredentials credentials) 
        {
            return Instance.Authenticate(challenge, request, credentials);
        }

        /// <devdoc>
        ///    <para>Pre-authenticates a request.</para>
        /// </devdoc>
        public static Authorization PreAuthenticate(WebRequest request, ICredentials credentials) 
        {
            return Instance.PreAuthenticate(request, credentials);
        }

        /// <devdoc>
        ///    <para>Registers an authentication module with the authentication manager.</para>
        /// </devdoc>
        public static void Register(IAuthenticationModule authenticationModule) 
        {
            ExceptionHelper.UnmanagedPermission.Demand();
            Instance.Register(authenticationModule);
        }

        /// <devdoc>
        ///    <para>Unregisters authentication modules for an authentication scheme.</para>
        /// </devdoc>
        public static void Unregister(IAuthenticationModule authenticationModule) 
        {
            ExceptionHelper.UnmanagedPermission.Demand();
            Instance.Unregister(authenticationModule);
        }

        /// <devdoc>
        ///    <para>Unregisters authentication modules for an authentication scheme.</para>
        /// </devdoc>
        public static void Unregister(string authenticationScheme) 
        {
            ExceptionHelper.UnmanagedPermission.Demand();
            Instance.Unregister(authenticationScheme);
        }

        /// <devdoc>
        ///    <para>
        ///       Returns a list of registered authentication modules.
        ///    </para>
        /// </devdoc>
        public static IEnumerator RegisteredModules 
        {
            get 
            {
                return Instance.RegisteredModules;
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
        internal static void BindModule(Uri uri, Authorization response, IAuthenticationModule module) 
        {
            Instance.BindModule(uri, response, module);
        }

        //
        // The method will extract the blob that does correspond to the moduled with the name passed in signature 
        // parameter. The method avoids confusion arisen from the parameters passed in a quoted string, such as:
        // WWW-Authenticate: Digest username="NTLM", realm="wit", NTLM ...
        //
        [SuppressMessage(
            "Microsoft.Globalization", "CA1308", Justification = "Assert-only by check for lower-case signature")]
        internal static int FindSubstringNotInQuotes(string challenge, string signature) 
        {
            int index = -1;
            Debug.Assert(signature.ToLowerInvariant().Equals(signature, StringComparison.Ordinal),
                "'signature' parameter must be lower case");
            if (challenge != null && signature != null && challenge.Length >= signature.Length)
            {
                int firstQuote = -1, secondQuote = -1;
                for (int i = 0; i < challenge.Length && index < 0; i++)
                {
                    // Search for the quotes
                    if (challenge[i] == '\"')
                    {
                        if (firstQuote <= secondQuote)
                            firstQuote = i;
                        else
                            secondQuote = i;
                    }
                    // We've found both ends of an unquoted segment (could be whole challenge), search inside for the signature.
                    if (i == challenge.Length - 1 || (challenge[i] == '\"' && firstQuote > secondQuote))
                    {
                        // see if the portion of challenge out of the quotes contains
                        // the signature of the IAuthenticationModule
                        if (i == challenge.Length - 1)
                            firstQuote = challenge.Length;
                        // unquoted segment is too small to hold a scheme name, ie: scheme param="value",a=""
                        if (firstQuote < secondQuote + 3)
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
                    if ((challenge[start + i] | 0x20) != lwrCaseSignature[i])
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
        internal static int SplitNoQuotes(string challenge, ref int offset) 
        {
            // GlobalLog.Print("SplitNoQuotes([" + challenge + "], " + offset.ToString() + ")");
            //
            // save offset
            //
            int realOffset = offset;
            //
            // default is not found
            //
            offset = -1;

            if (challenge != null && realOffset < challenge.Length)
            {
                int firstQuote = -1, secondQuote = -1;

                for (int i = realOffset; i < challenge.Length; i++)
                {
                    //
                    // firstQuote>secondQuote means we are in a quoted string
                    //
                    if (firstQuote > secondQuote && challenge[i] == '\\' && i + 1 < challenge.Length && challenge[i + 1] == '\"')
                    {
                        //
                        // skip <\"> when in a quoted string
                        //
                        i++;
                    }
                    else if (challenge[i] == '\"')
                    {
                        if (firstQuote <= secondQuote)
                        {
                            firstQuote = i;
                        }
                        else
                        {
                            secondQuote = i;
                        }
                    }
                    else if (challenge[i] == '=' && firstQuote <= secondQuote && offset < 0)
                    {
                        offset = i;
                    }
                    else if (challenge[i] == ',' && firstQuote <= secondQuote)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

#if !FEATURE_PAL
        internal static Authorization GetGroupAuthorization(
            IAuthenticationModule thisModule, 
            string token, 
            bool finished, 
            NTAuthentication authSession, 
            bool shareAuthenticatedConnections, 
            bool mutualAuth) 
        {
            return new Authorization(
                    token,
                    finished,
                    (shareAuthenticatedConnections) ? null 
                        : (thisModule.GetType().FullName + "/" + authSession.UniqueUserId),
                    mutualAuth);
        }
#endif // !FEATURE_PAL

    } // class AuthenticationManager

} // namespace System.Net
