//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Metadata
{
    /// <summary>
    /// Defines the key type enumeration for <see cref="KeyDescriptor"/>.
    /// </summary>
    public enum KeyType
    {
#pragma warning disable 1591
        Unspecified = 0,
        Signing = 1,
        Encryption = 2,
#pragma warning restore 1591
    }
}
