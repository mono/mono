//------------------------------------------------------------------------------
// <copyright file="SelectingProviderEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.ApplicationServices
{
    using System;
    using System.Security.Principal;
    using System.Web;

    public class SelectingProviderEventArgs : EventArgs
    {
        private IPrincipal _user;
        public IPrincipal User {
            get { return _user; }
        }

        private string _providerName;
        public string ProviderName {
            get { return _providerName; }
            set { _providerName = value; }
        }

        internal SelectingProviderEventArgs(IPrincipal user, string providerName)
        {
            _user = user;
            _providerName = providerName;
        }

        //hiding default constructor
        private SelectingProviderEventArgs() { }
    }
}
