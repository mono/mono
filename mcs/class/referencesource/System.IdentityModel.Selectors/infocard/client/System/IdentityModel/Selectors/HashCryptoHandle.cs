//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.IdentityModel.Selectors
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;
    using IDT = Microsoft.InfoCards.Diagnostics.InfoCardTrace;

    //
    // Summary:
    //  Wraps a HashCryptoSession
    //
    internal class HashCryptoHandle : CryptoHandle
    {
        public HashCryptoHandle(InternalRefCountedHandle nativeHandle, DateTime expiration, IntPtr parameters)
            : base(nativeHandle, expiration, parameters, typeof(RpcHashCryptoParameters))
        {
        }

        private HashCryptoHandle(InternalRefCountedHandle internalHandle) : base(internalHandle) { }

        protected override CryptoHandle OnDuplicate()
        {
            return new HashCryptoHandle(InternalHandle);
        }

    }
}
