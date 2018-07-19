//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.IdentityModel.Tokens;

    class SecurityTokenContainer
    {
        SecurityToken token;

        public SecurityTokenContainer(SecurityToken token)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            this.token = token;
        }

        public SecurityToken Token
        {
            get { return this.token; }
        }
    }
}
