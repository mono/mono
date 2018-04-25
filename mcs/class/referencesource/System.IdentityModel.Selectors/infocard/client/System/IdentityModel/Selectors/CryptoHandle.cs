//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
//
// Presharp uses the c# pragma mechanism to supress its warnings.
// These are not recognised by the base compiler so we need to explictly
// disable the following warnings. See http://winweb/cse/Tools/PREsharp/userguide/default.asp 
// for details. 
//
#pragma warning disable 1634, 1691      // unknown message, unknown pragma

namespace System.IdentityModel.Selectors
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Security;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.CompilerServices;
    using IDT = Microsoft.InfoCards.Diagnostics.InfoCardTrace;


    //
    // For common & resources
    //
    using Microsoft.InfoCards;

    //
    // Summary:
    //  Wraps and manages the lifetime of a native crypto handle passed back from the native InfoCard API.
    //
    internal abstract class CryptoHandle : IDisposable
    {
        bool m_isDisposed;
        InternalRefCountedHandle m_internalHandle;

        //
        // Summary:
        //  Creates a new CryptoHandle. ParamType has information as to what
        //  nativeParameters has to be marshaled into.
        //
        protected CryptoHandle(InternalRefCountedHandle nativeHandle, DateTime expiration, IntPtr nativeParameters, Type paramType)
        {
            m_internalHandle = nativeHandle;

            m_internalHandle.Initialize(expiration, Marshal.PtrToStructure(nativeParameters, paramType));
        }

        //
        // Summary:
        //  This constructor creates a new CryptoHandle instance with the same InternalRefCountedHandle and adds
        //  a ref count to that InternalRefCountedHandle.
        //
        protected CryptoHandle(InternalRefCountedHandle internalHandle)
        {
            m_internalHandle = internalHandle;
            m_internalHandle.AddRef();
        }

        public InternalRefCountedHandle InternalHandle
        {
            get
            {
                ThrowIfDisposed();
                return m_internalHandle;
            }
        }



        public DateTime Expiration
        {
            get
            {
                ThrowIfDisposed();
                return m_internalHandle.Expiration;
            }
        }

        public object Parameters
        {
            get
            {
                ThrowIfDisposed();
                return m_internalHandle.Parameters;
            }
        }

        //
        // Summary:
        //  Creates a new CryptoHandle with same InternalRefCountedCryptoHandle.
        //
        public CryptoHandle Duplicate()
        {
            ThrowIfDisposed();
            return OnDuplicate();
        }


        //
        // Summary:
        //  Allows subclasses to create a duplicate of their particular class.
        //
        protected abstract CryptoHandle OnDuplicate();

        protected void ThrowIfDisposed()
        {
            if (m_isDisposed)
            {
                throw IDT.ThrowHelperError(new ObjectDisposedException(SR.GetString(SR.ClientCryptoSessionDisposed)));
            }
        }

        public void Dispose()
        {
            if (m_isDisposed)
            {
                return;
            }

            m_internalHandle.Release();
            m_internalHandle = null;
            m_isDisposed = true;
        }

        //
        // Summary:
        //  Given a pointer to a native cryptosession this method creates the appropriate CryptoHandle type.
        //
        static internal CryptoHandle Create(InternalRefCountedHandle nativeHandle)
        {
            CryptoHandle handle = null;

            bool mustRelease = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                nativeHandle.DangerousAddRef(ref mustRelease);
                RpcInfoCardCryptoHandle hCrypto =
                    (RpcInfoCardCryptoHandle)Marshal.PtrToStructure(nativeHandle.DangerousGetHandle(),
                                                                     typeof(RpcInfoCardCryptoHandle));
                DateTime expiration = DateTime.FromFileTimeUtc(hCrypto.expiration);

                switch (hCrypto.type)
                {
                    case RpcInfoCardCryptoHandle.HandleType.Asymmetric:
                        handle = new AsymmetricCryptoHandle(nativeHandle, expiration, hCrypto.cryptoParameters);
                        break;
                    case RpcInfoCardCryptoHandle.HandleType.Symmetric:
                        handle = new SymmetricCryptoHandle(nativeHandle, expiration, hCrypto.cryptoParameters);
                        break;
                    case RpcInfoCardCryptoHandle.HandleType.Transform:
                        handle = new TransformCryptoHandle(nativeHandle, expiration, hCrypto.cryptoParameters);
                        break;
                    case RpcInfoCardCryptoHandle.HandleType.Hash:
                        handle = new HashCryptoHandle(nativeHandle, expiration, hCrypto.cryptoParameters);
                        break;
                    default:
                        IDT.DebugAssert(false, "Invalid crypto operation type");
                        throw IDT.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.GeneralExceptionMessage)));
                }

                return handle;

            }
            finally
            {
                if (mustRelease)
                {
                    nativeHandle.DangerousRelease();
                }
            }
        }


    }

    //
    // Summary:
    //  This class manages the lifetime of a native crypto handle through ref counts.  Any number of CryptoHandles
    //  may refer to a single InternalRefCountedHandle, but once they are all disposed this object will dispose 
    //  itself as well.
    //
    internal class InternalRefCountedHandle : SafeHandle
    {
        int m_refcount = 0;
        DateTime m_expiration;
        object m_parameters = null;

        [DllImport("infocardapi.dll",
                    EntryPoint = "CloseCryptoHandle",
                    CharSet = CharSet.Unicode,
                    CallingConvention = CallingConvention.StdCall,
                    ExactSpelling = true,
                    SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static extern bool CloseCryptoHandle([In] IntPtr hKey);

        private InternalRefCountedHandle()
            : base(IntPtr.Zero, true)
        {
            m_refcount = 1;

        }

        public void Initialize(DateTime expiration, object parameters)
        {
            m_expiration = expiration;
            m_parameters = parameters;
        }


        //
        // Summary:
        //  The deserialized parameters specific to a particular type of CryptoHandle.
        //
        public object Parameters
        {
            get
            {
                ThrowIfInvalid();
                return m_parameters;
            }
        }

        //
        // Summary:
        //  The expiration of this CryptoHandle
        //
        public DateTime Expiration
        {
            get
            {
                ThrowIfInvalid();
                return m_expiration;
            }
        }

        public void AddRef()
        {
            ThrowIfInvalid();
            Interlocked.Increment(ref m_refcount);
        }

        public void Release()
        {
            ThrowIfInvalid();
            int refcount = Interlocked.Decrement(ref m_refcount);
            if (0 == refcount)
            {
                Dispose();
            }
        }

        private void ThrowIfInvalid()
        {
            if (IsInvalid)
            {
                throw IDT.ThrowHelperError(new ObjectDisposedException("InternalRefCountedHandle"));
            }
        }
        public override bool IsInvalid
        {
            get
            {
                return (IntPtr.Zero == base.handle);
            }
        }


        protected override bool ReleaseHandle()
        {
#pragma warning suppress 56523
            return CloseCryptoHandle(base.handle);
        }

    }
}
