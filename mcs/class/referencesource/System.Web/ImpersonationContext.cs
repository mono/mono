//------------------------------------------------------------------------------
// <copyright file="ImpersonationContext.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web {
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Util;

    internal class ImpersonationContext : IDisposable {
        private HandleRef _savedToken;
        private bool _reverted;
        private bool _impersonating;

        // arg-less ctor creates dummy context
        internal ImpersonationContext() {
        }

        // ctor that takes a token impersonates that token
        internal ImpersonationContext(IntPtr token) {
            ImpersonateToken(new HandleRef(this, token));
        }

        // IDisposable pattern

        ~ImpersonationContext() {
            Dispose(false);
        }

        void IDisposable.Dispose() {
            Undo();
        }

        private void Dispose(bool disposing) {
            if (_savedToken.Handle != IntPtr.Zero) {
                try {} finally {
                    UnsafeNativeMethods.CloseHandle(_savedToken.Handle);
                    _savedToken = new HandleRef(this, IntPtr.Zero);
                }
            }
        }

        // impersonate a given token
        protected void ImpersonateToken(HandleRef token) {
            try {
                // first revert
                _savedToken = new HandleRef(this, GetCurrentToken());

                if (_savedToken.Handle != IntPtr.Zero) {
                    if (UnsafeNativeMethods.RevertToSelf() != 0) {
                        _reverted = true;
                    }
                }

                // impersonate token if not zero
                if (token.Handle != IntPtr.Zero) { 
                    if (UnsafeNativeMethods.SetThreadToken(IntPtr.Zero, token.Handle) == 0) {
                        throw new HttpException(SR.GetString(SR.Cannot_impersonate));
                    }

                    _impersonating = true;
                }
            }
            catch {
                RestoreImpersonation();
                throw;
            }
        }

        // restore impersonation to the original state
        private void RestoreImpersonation() {
            // first revert before reimpersonating
            if (_impersonating) {
                UnsafeNativeMethods.RevertToSelf();
                _impersonating = false;
            }

            // second reimpersonate the orignal saved identity (if exists)
            if (_savedToken.Handle != IntPtr.Zero) {
                if (_reverted) {
                    if (UnsafeNativeMethods.SetThreadToken(IntPtr.Zero, _savedToken.Handle) == 0) {
                        throw new HttpException(SR.GetString(SR.Cannot_impersonate));
                    }
                }

                _reverted = false;
            }
        }

        // 'public' version of Dispose
        internal void Undo() {
            RestoreImpersonation();

            // free unmanaged resources
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        // helper to get the currently impersonated token
        private static IntPtr GetCurrentToken() {
            IntPtr token = IntPtr.Zero;

            if (UnsafeNativeMethods.OpenThreadToken(
                        UnsafeNativeMethods.GetCurrentThread(),
                        UnsafeNativeMethods.TOKEN_READ | UnsafeNativeMethods.TOKEN_IMPERSONATE,
                        true,
                        ref token) == 0) {

                // if the last error is ERROR_NO_TOKEN it is ok, otherwise throw
                if (Marshal.GetLastWin32Error() != UnsafeNativeMethods.ERROR_NO_TOKEN) {
                    throw new HttpException(SR.GetString(SR.Cannot_impersonate));
                }
            }

            return token;
        }

        // helper to check if there is a current token
        internal static bool CurrentThreadTokenExists {
            get {
                bool impersonating = false;
            
                try {} finally {
                   IntPtr token = GetCurrentToken();

                   if (token != IntPtr.Zero) {
                        impersonating = true;
                        UnsafeNativeMethods.CloseHandle(token);
                   }
                }

                return impersonating;
            }
        }
    }

    internal sealed class ProcessImpersonationContext : ImpersonationContext {
        internal ProcessImpersonationContext() {
            ImpersonateToken(new HandleRef(this, IntPtr.Zero));
        }
    }

    internal sealed class ApplicationImpersonationContext : ImpersonationContext {
        internal ApplicationImpersonationContext() {
            ImpersonateToken(new HandleRef(this, HostingEnvironment.ApplicationIdentityToken));
        }
    }

    internal sealed class ClientImpersonationContext : ImpersonationContext {
        internal ClientImpersonationContext(HttpContext context) {
            Start(context, true);
        }

        internal ClientImpersonationContext(HttpContext context, bool throwOnError) {
            Start(context, throwOnError);
        }

        private void Start(HttpContext context, bool throwOnError) {
            IntPtr token = IntPtr.Zero;

            try {
                if (context != null) {
                    token = context.ImpersonationToken;
                }
                else {
                    // by default use app identity
                    token = HostingEnvironment.ApplicationIdentityToken;
                }
            }
            catch {
                if (throwOnError) {
                    throw;
                }
            }

            // only impersonate if there is a token
            // this is to optimize for the default case of impersonation="false"
            // and no UNC identity, when requests run under process identity
            if (token != IntPtr.Zero) {
                ImpersonateToken(new HandleRef(this, token));
            }
        }
    }
}
