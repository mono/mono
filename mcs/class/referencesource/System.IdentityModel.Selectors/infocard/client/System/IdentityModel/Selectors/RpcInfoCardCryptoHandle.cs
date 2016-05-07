//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct RpcInfoCardCryptoHandle
    {
        //
        // NOTE!!!!!!!!!!
        // This enum must match up with the SessionType enum in servers CryptoSession class.
        // NOTE!!!!!!!!!!
        //
        public enum HandleType
        {
            Asymmetric = 1,
            Symmetric = 2,
            Transform = 3,
            Hash = 4
        };

        public HandleType type;
        public Int64 expiration;
        public IntPtr cryptoParameters;
    }
}

