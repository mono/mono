//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System.ServiceModel.Security;

    public enum PrincipalPermissionMode
    {
        None,
        UseWindowsGroups,
        UseAspNetRoles,
        Custom,
        Always
    }

    static class PrincipalPermissionModeHelper
    {
        public static bool IsDefined(PrincipalPermissionMode principalPermissionMode)
        {
            return Enum.IsDefined( typeof( PrincipalPermissionMode ), principalPermissionMode );
        }
    }
}
