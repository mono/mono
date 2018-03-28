//------------------------------------------------------------------------------
// <copyright file="IIS7User.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Hosting {
    using System;
    using System.Text;
    using System.Collections.Generic;
    using System.Security.Principal;

    internal sealed class IIS7UserPrincipal : IPrincipal {

        // user object fields
        private IIdentity _identity;
        private IIS7WorkerRequest _wr;

        internal IIS7UserPrincipal(IIS7WorkerRequest wr, IIdentity identity) {
            _wr = wr;
            _identity = identity;
        }

        //
        //  IPrincipal implementations
        //
        
        public IIdentity Identity {
            get { return _identity; }
        }

        public bool IsInRole(String role) {
            return _wr.IsUserInRole(role);
        }
    }
}
