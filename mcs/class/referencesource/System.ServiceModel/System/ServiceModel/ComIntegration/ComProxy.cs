//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    class ComProxy : IDisposable
    {
        IntPtr inner;
        IDisposable ccw;
        internal static ComProxy Create(IntPtr outer, object obj, IDisposable disp)
        {
            if (outer == IntPtr.Zero)
            {
                throw Fx.AssertAndThrow("Outer cannot be null");
            }
            IntPtr inner = IntPtr.Zero;
            inner = Marshal.CreateAggregatedObject(outer, obj);
            int refCount = Marshal.AddRef(inner);
            // Workaround for the CLR ref count issue. 
            if (3 == refCount)
                Marshal.Release(inner);
            Marshal.Release(inner);
            return new ComProxy(inner, disp);
        }

        internal ComProxy(IntPtr inner, IDisposable disp)
        {
            this.inner = inner;
            ccw = disp;
        }

        internal void QueryInterface(ref Guid riid, out IntPtr tearoff)
        {
            if (inner == IntPtr.Zero)
            {
                throw Fx.AssertAndThrow("Inner should not be Null at this point");
            }
            int hr = Marshal.QueryInterface(inner, ref riid, out tearoff);
            if (hr != HR.S_OK)
            {
                throw Fx.AssertAndThrow("QueryInterface should succeed");
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
        }

        void Dispose(bool disposing)
        {
            if (inner == IntPtr.Zero)
            {
                throw Fx.AssertAndThrow("Inner should not be Null at this point");
            }
            Marshal.Release(inner);
            if (disposing)
            {
                if (ccw != null)
                    ccw.Dispose();
            }
        }

        public ComProxy Clone()
        {
            if (inner == IntPtr.Zero)
            {
                throw Fx.AssertAndThrow("Inner should not be Null at this point");
            }
            Marshal.AddRef(inner);
            return new ComProxy(inner, null);
        }
    }
}
