//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Workflow.Activities
{
    using System.ServiceModel;
    using System.Security.Permissions;
    using System.Security;
    using System.Diagnostics.CodeAnalysis;

    class PrincipalPermissionServiceAuthorizationManager : ServiceAuthorizationManager
    {
        private PrincipalPermission principalPermission;

        public PrincipalPermissionServiceAuthorizationManager(PrincipalPermission principalPermission)
        {
            this.principalPermission = principalPermission;
        }

        [SuppressMessage("Reliability", "Reliability104")]
        public override bool CheckAccess(OperationContext operationContext)
        {
            bool approved = false;
            try
            {
                principalPermission.Demand();
                approved = true;
            }
            catch (SecurityException)
            {
            }

            return approved;
        }
    }
}
