//------------------------------------------------------------------------------
// <copyright file="ClientRolePrincipal.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Web.ClientServices
{
    using System;
    using System.Net;
    using System.Security.Principal;

    public class ClientRolePrincipal : IPrincipal
    {
        public IIdentity Identity { get { return _Identity; } }
        private IIdentity _Identity;

        public ClientRolePrincipal(IIdentity identity) {
            _Identity = identity;
        }
        public bool IsInRole(string role)
        {
            return System.Web.Security.Roles.IsUserInRole(_Identity.Name, role);
        }
    }
}
