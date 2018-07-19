//------------------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;

namespace System.Security.Claims
{
    public static class DynamicRoleClaimProvider
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use ClaimsAuthenticationManager to add claims to a ClaimsIdentity", true)]
        public static void AddDynamicRoleClaims(ClaimsIdentity claimsIdentity, IEnumerable<Claim> claims)
        {
            claimsIdentity.ExternalClaims.Add(claims);
        }
    }
}
