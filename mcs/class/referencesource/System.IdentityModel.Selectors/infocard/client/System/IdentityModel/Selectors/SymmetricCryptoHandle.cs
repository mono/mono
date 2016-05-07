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
    //  Wraps an SymmetricCryptoSession.
    //
    internal class SymmetricCryptoHandle : ProofTokenCryptoHandle
    {
        public SymmetricCryptoHandle(InternalRefCountedHandle nativeHandle, DateTime expiration, IntPtr parameters)
            : base(nativeHandle, expiration, parameters, typeof(RpcSymmetricCryptoParameters))
        {
        }

        private SymmetricCryptoHandle(InternalRefCountedHandle internalHandle) : base(internalHandle) { }

        protected override CryptoHandle OnDuplicate()
        {
            return new SymmetricCryptoHandle(InternalHandle);
        }

        protected override InfoCardProofToken OnCreateProofToken()
        {
            return new InfoCardProofToken(this, Expiration);
        }
    }
}

