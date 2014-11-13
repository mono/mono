//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;
    using System.Security.Authentication.ExtendedProtection;

    interface IChannelBindingProvider
    {
        void EnableChannelBindingSupport();
        bool IsChannelBindingSupportEnabled { get; }
    }
}
