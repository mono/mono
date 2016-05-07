//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Security.Tokens
{

    interface ISecurityContextSecurityTokenCacheProvider
    {
        ISecurityContextSecurityTokenCache TokenCache { get; }
    }
}
