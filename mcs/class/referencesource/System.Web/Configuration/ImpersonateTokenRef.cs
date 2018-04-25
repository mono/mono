//------------------------------------------------------------------------------
// <copyright file="ImpersonateTokenRef.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Xml;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Web.Util;
    using System.Web.Configuration;

    // class IdentitySection
    internal sealed class ImpersonateTokenRef : IDisposable {
        private IntPtr _handle;

        internal ImpersonateTokenRef(IntPtr token) {
            _handle = token;
        }

        internal IntPtr Handle {
            get { return _handle; }
        }

        // The handle can be kept alive by HttpContext.s_appIdentityConfig (see ASURT#121815)
        ~ImpersonateTokenRef() {
            if (_handle != IntPtr.Zero) {
                UnsafeNativeMethods.CloseHandle(_handle);
                _handle = IntPtr.Zero;
            }
        }
        void IDisposable.Dispose() {
            if (_handle != IntPtr.Zero) {
                UnsafeNativeMethods.CloseHandle(_handle);
                _handle = IntPtr.Zero;
            }
            GC.SuppressFinalize(this);
        }
    }
}
