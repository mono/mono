// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>[....]</OWNER>
// 
//
// RoleClaimProvider.cs
//

namespace System.Web.Security
{
    using System.Collections.Generic;
    using System.Security.Claims;

    /// <summary>
    /// This internal class is used to wrap role claims that are served up by the RolePrincipal.  They need to be kept distinct from other claims.
    /// ClaimsIdentity has a property the holds this type.
    /// made on parameters.
    /// </summary>    

    internal class RoleClaimProvider
    {
        RolePrincipal _rolePrincipal;
        ClaimsIdentity _subject;

        public RoleClaimProvider(RolePrincipal rolePrincipal, ClaimsIdentity subject)
        {
            _rolePrincipal  = rolePrincipal;
            _subject        = subject;
        }

        public IEnumerable<Claim> Claims
        {
            get
            {
                foreach (string role in _rolePrincipal.GetRoles())
                {
                    yield return new Claim(_subject.RoleClaimType, role, ClaimValueTypes.String, _rolePrincipal.ProviderName, _rolePrincipal.ProviderName, _subject);
                }
            }
        }
    }
}
