//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.IdentityModel.Selectors
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout( LayoutKind.Sequential )]
    internal struct RpcTransformCryptoParameters
    {
        public int inputBlockSize;
        public int outputBlockSize;
        public bool canTransformMultipleBlocks;
        public bool canReuseTransform;
    }
}

