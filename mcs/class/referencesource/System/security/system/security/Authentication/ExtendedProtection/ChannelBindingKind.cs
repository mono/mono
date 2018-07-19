//------------------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//
//------------------------------------------------------------------------------
namespace System.Security.Authentication.ExtendedProtection
{
    // These should match the native SEC_ATTR_*_BINDINGS defines
    public enum ChannelBindingKind
    {
        Unknown = 0,
        Unique = 0x19,
        Endpoint = 0x1A
    }
}
