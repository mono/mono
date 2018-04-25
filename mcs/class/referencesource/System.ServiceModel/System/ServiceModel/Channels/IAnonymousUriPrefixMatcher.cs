//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    public interface IAnonymousUriPrefixMatcher
    {
        void Register(Uri anonymousUriPrefix);
    }
}
