// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Channels
{
    using System.Security;
    using System.Security.Permissions;
    using Net;

    /// <summary>
    /// Implementation of the IHttpCookieContainerManager
    /// </summary>
    class HttpCookieContainerManager : IHttpCookieContainerManager
    {
        private CookieContainer cookieContainer;

        // We need this flag to avoid overriding the CookieConatiner if the user has already initialized it.
        public bool IsInitialized { get; private set; }

        public CookieContainer CookieContainer
        {
            get
            {
                return this.cookieContainer;
            }

            set
            {
                this.IsInitialized = true;
                this.cookieContainer = value;
            }
        }
    } 
}
