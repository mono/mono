// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// <OWNER>[....]</OWNER>

using System;
using System.Security;
using System.Security.Permissions;

namespace System.Security
{
    [System.Security.SecurityCritical]  // auto-generated_required
#pragma warning disable 618
    [PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
#pragma warning restore 618
    public abstract class SecurityState
    {
        protected SecurityState(){}
        
        [System.Security.SecurityCritical]  // auto-generated
        public bool IsStateAvailable()
        {
            AppDomainManager domainManager = AppDomainManager.CurrentAppDomainManager;
            return domainManager != null ? domainManager.CheckSecuritySettings(this) : false;
        }
        // override this function and throw the appropriate 
        public abstract void EnsureState();
    }

}
