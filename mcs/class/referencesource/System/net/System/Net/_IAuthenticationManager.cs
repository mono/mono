//------------------------------------------------------------------------------
// <copyright file="_IAuthenticationManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net 
{
    using System.Collections;
    using System.Collections.Specialized;
    using System.Net;

    internal interface IAuthenticationManager 
    {
        ICredentialPolicy CredentialPolicy 
        { 
            get; 
            set; 
        }

        StringDictionary CustomTargetNameDictionary 
        { 
            get; 
        }

        SpnDictionary SpnDictionary 
        { 
            get; 
        }

        bool OSSupportsExtendedProtection 
        { 
            get; 
        }

        bool SspSupportsExtendedProtection 
        { 
            get; 
        }

        void EnsureConfigLoaded();

        Authorization Authenticate(string challenge, WebRequest request, ICredentials credentials);

        Authorization PreAuthenticate(WebRequest request, ICredentials credentials);

        void Register(IAuthenticationModule authenticationModule);

        void Unregister(IAuthenticationModule authenticationModule);

        void Unregister(string authenticationScheme);

        IEnumerator RegisteredModules 
        { 
            get; 
        }

        void BindModule(Uri uri, Authorization response, IAuthenticationModule module);
    } // class AuthenticationManager
} // namespace System.Net
