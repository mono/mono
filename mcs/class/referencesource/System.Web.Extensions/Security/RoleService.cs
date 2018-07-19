//------------------------------------------------------------------------------
// <copyright file="RoleService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Security {
    using System;
    using System.Web.ApplicationServices;
    using System.Web.Script.Services;
    using System.Web.Services;

    [ScriptService]
    internal sealed class RoleService {
        [WebMethod]
        public string[] GetRolesForCurrentUser(){
            ApplicationServiceHelper.EnsureRoleServiceEnabled();
            return Roles.GetRolesForUser();
        }

        [WebMethod]
        public bool IsCurrentUserInRole(string role) {
            if (role == null) {
                throw new ArgumentNullException("role");
            }
            
            ApplicationServiceHelper.EnsureRoleServiceEnabled();
            return Roles.IsUserInRole(role);
        }
    }
}
