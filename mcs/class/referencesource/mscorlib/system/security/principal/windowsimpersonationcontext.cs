// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>Microsoft</OWNER>
// 

//
// WindowsImpersonationContext.cs
//
// Representation of an impersonation context.
//

namespace System.Security.Principal
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System.Runtime.InteropServices;
#if FEATURE_CORRUPTING_EXCEPTIONS
    using System.Runtime.ExceptionServices;
#endif // FEATURE_CORRUPTING_EXCEPTIONS
    using System.Security.Permissions;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.Versioning;
    using System.Diagnostics.Contracts;

    [System.Runtime.InteropServices.ComVisible(true)]
    public class WindowsImpersonationContext : IDisposable {
        [System.Security.SecurityCritical] // auto-generated
        private SafeAccessTokenHandle m_safeTokenHandle = SafeAccessTokenHandle.InvalidHandle;
        private WindowsIdentity m_wi;
        private FrameSecurityDescriptor m_fsd;

        [System.Security.SecurityCritical]  // auto-generated
        private WindowsImpersonationContext () {}

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        internal WindowsImpersonationContext (SafeAccessTokenHandle safeTokenHandle, WindowsIdentity wi, bool isImpersonating, FrameSecurityDescriptor fsd) {
            if (safeTokenHandle.IsInvalid)
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidImpersonationToken"));
            Contract.EndContractBlock();

            if (isImpersonating) {
                if (!Win32Native.DuplicateHandle(Win32Native.GetCurrentProcess(),
                                                 safeTokenHandle,
                                                 Win32Native.GetCurrentProcess(),
                                                 ref m_safeTokenHandle,
                                                 0,
                                                 true,
                                                 Win32Native.DUPLICATE_SAME_ACCESS))
                    throw new SecurityException(Win32Native.GetMessage(Marshal.GetLastWin32Error()));
                m_wi = wi;
            }
            m_fsd = fsd;
        }

        // Revert to previous impersonation (the only public method).
        [System.Security.SecuritySafeCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
        public void Undo () {
            int hr = 0;
            if (m_safeTokenHandle.IsInvalid) { // the thread was not initially impersonating
                hr = Win32.RevertToSelf();
                if (hr < 0)
                    Environment.FailFast(Win32Native.GetMessage(hr));
            } else {
                hr = Win32.RevertToSelf();
                if (hr < 0)
                    Environment.FailFast(Win32Native.GetMessage(hr));
                hr = Win32.ImpersonateLoggedOnUser(m_safeTokenHandle);
                if (hr < 0)
                    throw new SecurityException(Win32Native.GetMessage(hr));
            }
            WindowsIdentity.UpdateThreadWI(m_wi);
            if (m_fsd != null)
                m_fsd.SetTokenHandles(null, null);
        }

        // Non-throwing version that does not new any exception objects. To be called when reliability matters
        [System.Security.SecurityCritical]  // auto-generated
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
#if FEATURE_CORRUPTING_EXCEPTIONS
        [HandleProcessCorruptedStateExceptions] // 
#endif // FEATURE_CORRUPTING_EXCEPTIONS
        internal bool UndoNoThrow()
        {
            bool bRet = false;
            try{
                int hr = 0;
                if (m_safeTokenHandle.IsInvalid) 
                { // the thread was not initially impersonating
                    hr = Win32.RevertToSelf();
                    if (hr < 0)
                        Environment.FailFast(Win32Native.GetMessage(hr));
                } 
                else
                {
                    hr = Win32.RevertToSelf();
                    if (hr >= 0)
                    {
                        hr = Win32.ImpersonateLoggedOnUser(m_safeTokenHandle);
                    }
                    else
                    {
                        Environment.FailFast(Win32Native.GetMessage(hr));
                    }
                }
                bRet = (hr >= 0);
                if (m_fsd != null)
                    m_fsd.SetTokenHandles(null,null);
            }
            catch
            {
                bRet = false;
            }
            return bRet;
        }

        //
        // IDisposable interface.
        //

        [System.Security.SecuritySafeCritical]  // auto-generated
        [ComVisible(false)]
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (m_safeTokenHandle != null && !m_safeTokenHandle.IsClosed) {
                    Undo();
                    m_safeTokenHandle.Dispose();
                }
            }
        }

        [ComVisible(false)]
        public void Dispose () {
            Dispose(true);
        }
    }
}
