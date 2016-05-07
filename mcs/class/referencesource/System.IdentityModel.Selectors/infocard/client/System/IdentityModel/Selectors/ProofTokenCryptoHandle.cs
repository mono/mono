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
    //  InfoCardProofTokens can be created from this kind of CryptoHandle.
    //
    internal abstract class ProofTokenCryptoHandle : CryptoHandle
    {
        protected ProofTokenCryptoHandle(InternalRefCountedHandle nativeHandle, DateTime expiration, IntPtr nativeParameters, Type paramType)
            : base(nativeHandle, expiration, nativeParameters, paramType)
        {
        }

        protected ProofTokenCryptoHandle(InternalRefCountedHandle internalHandle)
            : base(internalHandle) { }

        //
        // Summary:
        //  Creates a new InfoCardProofToken from the underlying CryptoHandle.
        //
        public InfoCardProofToken CreateProofToken()
        {
            ThrowIfDisposed();
            return OnCreateProofToken();
        }

        //
        // Summary:
        //  Allows subclasses to create their particular proof token.
        //
        protected abstract InfoCardProofToken OnCreateProofToken();
    }
}
