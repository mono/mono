//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System.IdentityModel.Tokens;

    interface IWrappedTokenKeyResolver
    {
        SecurityToken ExpectedWrapper
        {
            get;
            set;
        }

        bool CheckExternalWrapperMatch(SecurityKeyIdentifier keyIdentifier);
    }
}
