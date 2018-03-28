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
    // For common & resources
    //
    using Microsoft.InfoCards;

    //
    // Summary:
    //  Wraps an AsymmetricCryptoSession.
    //
    internal class AsymmetricCryptoHandle : ProofTokenCryptoHandle
    {
        public AsymmetricCryptoHandle(InternalRefCountedHandle nativeHandle, DateTime expiration, IntPtr parameters)
            : base(nativeHandle, expiration, parameters, typeof(RpcAsymmetricCryptoParameters))
        {
        }

        private AsymmetricCryptoHandle(InternalRefCountedHandle internalHandle) : base(internalHandle) { }


        protected override CryptoHandle OnDuplicate()
        {
            return new AsymmetricCryptoHandle(InternalHandle);
        }

        protected override InfoCardProofToken OnCreateProofToken()
        {
            return new InfoCardProofToken(this, Expiration);
        }
    }
}

