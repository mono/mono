//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security.AccessControl;
    using System.Security.Principal;
    using System.ServiceModel.Diagnostics;
    using System.Runtime.Versioning;

    class Privilege
    {
        static Dictionary<string, LUID> luids = new Dictionary<string, LUID>();
        public const string SeAuditPrivilege = "SeAuditPrivilege";
        public const string SeTcbPrivilege = "SeTcbPrivilege";

        const uint SE_PRIVILEGE_DISABLED = 0x00000000;
        const uint SE_PRIVILEGE_ENABLED_BY_DEFAULT = 0x00000001;
        const uint SE_PRIVILEGE_ENABLED = 0x00000002;
        const uint SE_PRIVILEGE_USED_FOR_ACCESS = 0x80000000;
        const int ERROR_SUCCESS = 0x0;
        const int ERROR_NO_TOKEN = 0x3F0;
        const int ERROR_NOT_ALL_ASSIGNED = 0x514;

        string privilege;
        LUID luid;
        bool needToRevert = false;
        bool initialEnabled = false;
        bool isImpersonating = false;
        SafeCloseHandle threadToken = null;

        public Privilege(string privilege)
        {
            this.privilege = privilege;
            this.luid = LuidFromPrivilege(privilege);
        }

        public void Enable()
        {
            // Note: AdjustTokenPrivileges should not try to adjust if the token is
            // Primary token (process).  Duplicate the process token (impersonation) and 
            // then set token to current thread  and unsetting (RevertToSelf) later.
            DiagnosticUtility.DebugAssert(this.threadToken == null, "");
            this.threadToken = GetThreadToken();
            EnableTokenPrivilege(this.threadToken);
        }

        // Have to run in CER
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public int Revert()
        {
            if (!this.isImpersonating)
            {
                if (this.needToRevert && !this.initialEnabled)
                {
                    TOKEN_PRIVILEGE newState;
                    newState.PrivilegeCount = 1;
                    newState.Privilege.Luid = this.luid;
                    newState.Privilege.Attributes = SE_PRIVILEGE_DISABLED;

                    TOKEN_PRIVILEGE previousState;
                    uint previousSize = 0;

                    if (!NativeMethods.AdjustTokenPrivileges(
                                      this.threadToken,
                                      false,
                                      ref newState,
                                      TOKEN_PRIVILEGE.Size,
                                      out previousState,
                                      out previousSize))
                    {
                        return Marshal.GetLastWin32Error();
                    }
                }
                this.needToRevert = false;
            }
            else
            {
                if (!NativeMethods.RevertToSelf())
                {
                    return Marshal.GetLastWin32Error();
                }
                this.isImpersonating = false;
            }

            if (this.threadToken != null)
            {
                this.threadToken.Close();
                this.threadToken = null;
            }

            return ERROR_SUCCESS;
        }

        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption( ResourceScope.Process, ResourceScope.Process )]
        SafeCloseHandle GetThreadToken()
        {
            //
            // Open the thread token; if there is no thread token, get one from
            // the process token by impersonating self.
            //
            SafeCloseHandle threadToken;
            if (!NativeMethods.OpenThreadToken(
                            NativeMethods.GetCurrentThread(),
                            TokenAccessLevels.Query | TokenAccessLevels.AdjustPrivileges,
                            true,
                            out threadToken))
            {
                int error = Marshal.GetLastWin32Error();
                Utility.CloseInvalidOutSafeHandle(threadToken);
                if (error != ERROR_NO_TOKEN)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
                }
                else
                {
                    SafeCloseHandle processToken;
                    if (!NativeMethods.OpenProcessToken(
                                    NativeMethods.GetCurrentProcess(),
                                    TokenAccessLevels.Duplicate,
                                    out processToken))
                    {
                        error = Marshal.GetLastWin32Error();
                        Utility.CloseInvalidOutSafeHandle(processToken);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
                    }

                    try
                    {
                        if (!NativeMethods.DuplicateTokenEx(
                                            processToken,
                                            TokenAccessLevels.Impersonate | TokenAccessLevels.Query | TokenAccessLevels.AdjustPrivileges,
                                            IntPtr.Zero,
                                            SECURITY_IMPERSONATION_LEVEL.Impersonation,
                                            TokenType.TokenImpersonation,
                                            out threadToken))
                        {
                            error = Marshal.GetLastWin32Error();
                            Utility.CloseInvalidOutSafeHandle(threadToken);
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
                        }

                        SetThreadToken(threadToken);
                    }
                    finally
                    {
                        processToken.Close();
                    }
                }
            }
            return threadToken;
        }

        void EnableTokenPrivilege(SafeCloseHandle threadToken)
        {
            DiagnosticUtility.DebugAssert(!this.needToRevert, "");
            TOKEN_PRIVILEGE newState;
            newState.PrivilegeCount = 1;
            newState.Privilege.Luid = this.luid;
            newState.Privilege.Attributes = SE_PRIVILEGE_ENABLED;

            TOKEN_PRIVILEGE previousState;
            uint previousSize = 0;
            bool success = false;
            int error = 0;

            // CER
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                success = NativeMethods.AdjustTokenPrivileges(
                                  threadToken,
                                  false,
                                  ref newState,
                                  TOKEN_PRIVILEGE.Size,
                                  out previousState,
                                  out previousSize);

                error = Marshal.GetLastWin32Error();
                if (success && error == ERROR_SUCCESS)
                {
                    this.initialEnabled = (0 != (previousState.Privilege.Attributes & SE_PRIVILEGE_ENABLED));
                    this.needToRevert = true;
                }
            }

            if (error == ERROR_NOT_ALL_ASSIGNED)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new PrivilegeNotHeldException(this.privilege));
            }
            else if (!success)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
            }
        }

        void SetThreadToken(SafeCloseHandle threadToken)
        {
            DiagnosticUtility.DebugAssert(!this.isImpersonating, "");
            int error = 0;
            // CER
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                if (!NativeMethods.SetThreadToken(IntPtr.Zero, threadToken))
                {
                    error = Marshal.GetLastWin32Error();
                }
                else
                {
                    this.isImpersonating = true;
                }
            }
            if (!this.isImpersonating)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
            }
        }

        static LUID LuidFromPrivilege(string privilege)
        {
            LUID luid;
            lock (luids)
            {
                if (luids.TryGetValue(privilege, out luid))
                {
                    return luid;
                }
            }

            if (!NativeMethods.LookupPrivilegeValueW(null, privilege, out luid))
            {
                int error = Marshal.GetLastWin32Error();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
            }

            lock (luids)
            {
                if (!luids.ContainsKey(privilege))
                {
                    luids[privilege] = luid;
                }
            }

            return luid;
        }
    }
}
