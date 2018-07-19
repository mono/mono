//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    using System.IdentityModel.Tokens;

    static class EmptySecurityTokenResolver
    {
        static readonly SecurityTokenResolver _instance 
            = SecurityTokenResolver.CreateDefaultSecurityTokenResolver( EmptyReadOnlyCollection<SecurityToken>.Instance, false );

        public static SecurityTokenResolver Instance { get { return _instance; } }
    }
}
