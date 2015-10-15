//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Activation
{
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Principal;
    using System.ServiceModel.Activation.Interop;
    using System.Threading;

    [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - All member methods are security critical. The class might be used outside of the restricted SecurityContext." +
        "Ensure they do not accidentally satisfy any demands.")]
    class HostedImpersonationContext
    {
        [Fx.Tag.SecurityNote(Critical = "Stores the impersonation token handle. Since we're allowing 'safe' Impersonation of the token we got from asp.net we need to protect this value.")]
        [SecurityCritical]
        SafeCloseHandleCritical tokenHandle;

        [Fx.Tag.SecurityNote(Critical = "Controls lifetime of token handle, caller must use care.")]
        [SecurityCritical]
        int refCount = 0;

        [Fx.Tag.SecurityNote(Critical = "Security critical field, caller must use care.")]
        [SecurityCritical]
        bool isImpersonated;

        [Fx.Tag.SecurityNote(Critical = "Calls two safe native methods under OpenCurrentThreadTokenCritical: GetCurrentThread and OpenThreadToken." +
            "Marshal.GetLastWin32Error captures current thread token in a SecurityCritical field.")]
        [SecurityCritical]
        public HostedImpersonationContext()
        {
            if (ServiceHostingEnvironment.AspNetCompatibilityEnabled)
            {
                int error;
                bool isSuccess = SafeNativeMethods.OpenCurrentThreadTokenCritical(TokenAccessLevels.Query | TokenAccessLevels.Impersonate,
                    true, out tokenHandle, out error);

                if (isSuccess)
                {
                    isImpersonated = true;
                    Interlocked.Increment(ref refCount);
                }
                else
                {
                    CloseInvalidOutSafeHandleCritical(tokenHandle);
                    tokenHandle = null;
                    if (error != SafeNativeMethods.ERROR_NO_TOKEN)
                    {
                        throw FxTrace.Exception.AsError(new Win32Exception(error, SR.Hosting_ImpersonationFailed));
                    }
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Calls SetHandleAsInvalid which has a LinkDemand for UnmanagedCode.")]
        [SecurityCritical]
        static void CloseInvalidOutSafeHandleCritical(SafeHandle handle)
        {
            // Workaround for 64-bit CLR 
            if (handle != null)
            {
                Fx.Assert(handle.IsInvalid, "CloseInvalidOutSafeHandle called with a valid handle!");

                // Calls SuppressFinalize.
                handle.SetHandleAsInvalid();
            }
        }


        public bool IsImpersonated
        {
            [Fx.Tag.SecurityNote(Critical = "Accesses SecurityCritical isImpersonated field.", Safe = "Does not leak control or mutable/harmful data, no potential for harm.")]
            [SecuritySafeCritical]
            get
            {
                return isImpersonated;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses SecurityCritical tokenHandle field and uses LinkDemanded DangerousGetHandle method as well as UnsafeCreate." +
            "Caller should use with care, must take responsibility for reverting impersonation.")]
        [SecurityCritical]
        public IDisposable Impersonate()
        {
            if (!isImpersonated)
                return null;

            Fx.Assert(tokenHandle != null, "The token handle was incorrectly released earlier.");
            HostedInnerImpersonationContext context = null;
            lock (tokenHandle)
            {
                context = HostedInnerImpersonationContext.UnsafeCreate(tokenHandle.DangerousGetHandle());
                GC.KeepAlive(tokenHandle);
            }
            return context;
        }

        [Fx.Tag.SecurityNote(Critical = "Controls lifetime of token handle, caller must use care.")]
        [SecurityCritical]
        public void AddRef()
        {
            if (IsImpersonated)
            {
                Interlocked.Increment(ref refCount);
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Controls lifetime of token handle, caller must use care.")]
        [SecurityCritical]
        public void Release()
        {
            if (IsImpersonated)
            {
                Fx.Assert(tokenHandle != null, "The token handle is incorrectly released earlier.");
                int currentCount = Interlocked.Decrement(ref refCount);
                if (currentCount == 0)
                {
                    lock (tokenHandle)
                    {
                        tokenHandle.Close();
                        tokenHandle = null;
                    }
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Keeps track of impersonated user, caller must use with care and call Dispose at the appropriate time.")]
#pragma warning disable 618 // have not moved to the v4 security model yet
        [SecurityCritical(SecurityCriticalScope.Everything)]
#pragma warning restore 618
        class HostedInnerImpersonationContext : IDisposable
        {
            IDisposable impersonatedContext;

            HostedInnerImpersonationContext(IDisposable impersonatedContext)
            {
                if (impersonatedContext == null)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.Hosting_ImpersonationFailed));
                }
                this.impersonatedContext = impersonatedContext;
            }

            public static HostedInnerImpersonationContext UnsafeCreate(IntPtr token)
            {
                return new HostedInnerImpersonationContext(HostingEnvironmentWrapper.UnsafeImpersonate(token));
            }

            public void Dispose()
            {
                if (impersonatedContext != null)
                {
                    impersonatedContext.Dispose();
                    impersonatedContext = null;
                }
            }
        }
    }
}
