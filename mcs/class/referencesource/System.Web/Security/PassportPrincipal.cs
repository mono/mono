//------------------------------------------------------------------------------
// <copyright file="PassportPrincipal.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * PassportPrincipal
 * 
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web.Security {
    using System.Security.Principal;

    [Obsolete("This type is obsolete. The Passport authentication product is no longer supported and has been superseded by Live ID.")]
    public sealed class PassportPrincipal : GenericPrincipal {
        public PassportPrincipal(PassportIdentity identity, string[] roles) : base(identity, roles) 
        { }
    }
}
