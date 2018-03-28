//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Runtime.CompilerServices;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.Web.Security;

    sealed class RoleProviderPrincipal : IPrincipal
    {
        object roleProvider;
        ServiceSecurityContext securityContext;

        public RoleProviderPrincipal(object roleProvider, ServiceSecurityContext securityContext)
        {
            this.roleProvider = roleProvider;
            this.securityContext = securityContext;
        }

        public IIdentity Identity
        {
            get { return this.securityContext.PrimaryIdentity; }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public bool IsInRole(string role)
        {
            RoleProvider roleProvider = (this.roleProvider as RoleProvider) ?? SystemWebHelper.GetDefaultRoleProvider();
            if (roleProvider != null)
            {
                return roleProvider.IsUserInRole(this.securityContext.PrimaryIdentity.Name, role);
            }
            return false;
        }
    }
}
