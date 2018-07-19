//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Activation
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.AccessControl;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.ComIntegration;
    using System.Text;

    unsafe static class Utility
    {
        const string WindowsServiceAccountFormat = "NT Service\\{0}";

        internal static Uri FormatListenerEndpoint(string serviceName, string listenerEndPoint)
        {
            UriBuilder builder = new UriBuilder(Uri.UriSchemeNetPipe, serviceName);
            builder.Path = string.Format(CultureInfo.InvariantCulture, "/{0}/", listenerEndPoint);
            return builder.Uri;
        }

        static SafeCloseHandle OpenCurrentProcessForWrite()
        {
            int processId = Process.GetCurrentProcess().Id;
#pragma warning suppress 56523 // Microsoft, Win32Exception ctor calls Marshal.GetLastWin32Error()
            SafeCloseHandle process = ListenerUnsafeNativeMethods.OpenProcess(ListenerUnsafeNativeMethods.PROCESS_QUERY_INFORMATION | ListenerUnsafeNativeMethods.WRITE_DAC | ListenerUnsafeNativeMethods.READ_CONTROL, false, processId);
            if (process.IsInvalid)
            {
                Exception exception = new Win32Exception();
                process.SetHandleAsInvalid();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
            }
            return process;
        }

        static SafeCloseHandle OpenProcessForQuery(int pid)
        {
#pragma warning suppress 56523 // Microsoft, Win32Exception ctor calls Marshal.GetLastWin32Error()
            SafeCloseHandle process = ListenerUnsafeNativeMethods.OpenProcess(ListenerUnsafeNativeMethods.PROCESS_QUERY_INFORMATION, false, pid);
            if (process.IsInvalid)
            {
                Exception exception = new Win32Exception();
                process.SetHandleAsInvalid();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
            }
            return process;
        }

        static SafeCloseHandle GetProcessToken(SafeCloseHandle process, int requiredAccess)
        {
            SafeCloseHandle processToken;
            bool success = ListenerUnsafeNativeMethods.OpenProcessToken(process, requiredAccess, out processToken);
            int error = Marshal.GetLastWin32Error();
            if (!success)
            {
                System.ServiceModel.Diagnostics.Utility.CloseInvalidOutSafeHandle(processToken);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
            }

            return processToken;
        }

        static int GetTokenInformationLength(SafeCloseHandle token, ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS tic)
        {
            int lengthNeeded;
            bool success = ListenerUnsafeNativeMethods.GetTokenInformation(token, tic, null, 0, out lengthNeeded);
            if (!success)
            {
                int error = Marshal.GetLastWin32Error();
                if (error != ListenerUnsafeNativeMethods.ERROR_INSUFFICIENT_BUFFER)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
                }
            }

            return lengthNeeded;
        }

        static void GetTokenInformation(SafeCloseHandle token, ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS tic, byte[] tokenInformation)
        {
            int lengthNeeded;
            if (!ListenerUnsafeNativeMethods.GetTokenInformation(token, tic, tokenInformation, tokenInformation.Length, out lengthNeeded))
            {
                int error = Marshal.GetLastWin32Error();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
            }
        }

        static SafeServiceHandle OpenSCManager()
        {
#pragma warning suppress 56523 // Microsoft, Win32Exception ctor calls Marshal.GetLastWin32Error()
            SafeServiceHandle scManager = ListenerUnsafeNativeMethods.OpenSCManager(null, null, ListenerUnsafeNativeMethods.SC_MANAGER_CONNECT);
            if (scManager.IsInvalid)
            {
                Exception exception = new Win32Exception();
                scManager.SetHandleAsInvalid();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
            }
            return scManager;
        }

        static SafeServiceHandle OpenService(SafeServiceHandle scManager, string serviceName, int purpose)
        {
#pragma warning suppress 56523 // Microsoft, Win32Exception ctor calls Marshal.GetLastWin32Error()
            SafeServiceHandle service = ListenerUnsafeNativeMethods.OpenService(scManager, serviceName, purpose);
            if (service.IsInvalid)
            {
                Exception exception = new Win32Exception();
                service.SetHandleAsInvalid();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
            }
            return service;
        }

        internal static void AddRightGrantedToAccounts(List<SecurityIdentifier> accounts, int right, bool onProcess)
        {
            SafeCloseHandle process = OpenCurrentProcessForWrite();
            try
            {
                if (onProcess)
                {
                    EditKernelObjectSecurity(process, accounts, null, right, true);
                }
                else
                {
                    SafeCloseHandle token = GetProcessToken(process, ListenerUnsafeNativeMethods.TOKEN_QUERY | ListenerUnsafeNativeMethods.WRITE_DAC | ListenerUnsafeNativeMethods.READ_CONTROL);
                    try
                    {
                        EditKernelObjectSecurity(token, accounts, null, right, true);
                    }
                    finally
                    {
                        token.Close();
                    }
                }
            }
            finally
            {
                process.Close();
            }
        }

        internal static void AddRightGrantedToAccount(SecurityIdentifier account, int right)
        {
            SafeCloseHandle process = OpenCurrentProcessForWrite();
            try
            {
                EditKernelObjectSecurity(process, null, account, right, true);
            }
            finally
            {
                process.Close();
            }
        }

        internal static void RemoveRightGrantedToAccount(SecurityIdentifier account, int right)
        {
            SafeCloseHandle process = OpenCurrentProcessForWrite();
            try
            {
                EditKernelObjectSecurity(process, null, account, right, false);
            }
            finally
            {
                process.Close();
            }
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        internal static void KeepOnlyPrivilegeInProcess(string privilege)
        {
            SafeCloseHandle process = OpenCurrentProcessForWrite();
            try
            {
                SafeCloseHandle token = GetProcessToken(process, ListenerUnsafeNativeMethods.TOKEN_QUERY | ListenerUnsafeNativeMethods.TOKEN_ADJUST_PRIVILEGES | ListenerUnsafeNativeMethods.READ_CONTROL);
                try
                {
                    LUID luid;
                    bool success = ListenerUnsafeNativeMethods.LookupPrivilegeValue(IntPtr.Zero, privilege, &luid);
                    if (!success)
                    {
                        int error = Marshal.GetLastWin32Error();
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
                    }

                    int length = GetTokenInformationLength(token, ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS.TokenPrivileges);
                    byte[] tokenInformation = new byte[length];
                    fixed (byte* pTokenPrivileges = tokenInformation)
                    {
                        GetTokenInformation(token, ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS.TokenPrivileges,
                            tokenInformation);

                        ListenerUnsafeNativeMethods.TOKEN_PRIVILEGES* pTP = (ListenerUnsafeNativeMethods.TOKEN_PRIVILEGES*)pTokenPrivileges;
                        LUID_AND_ATTRIBUTES* pLuidAndAttributes = (LUID_AND_ATTRIBUTES*)(&(pTP->Privileges));
                        int privilegeCount = 0;
                        for (int i = 0; i < pTP->PrivilegeCount; i++)
                        {
                            if (!pLuidAndAttributes[i].Luid.Equals(luid))
                            {
                                pLuidAndAttributes[privilegeCount].Attributes = PrivilegeAttribute.SE_PRIVILEGE_REMOVED;
                                pLuidAndAttributes[privilegeCount].Luid = pLuidAndAttributes[i].Luid;
                                privilegeCount++;
                            }
                        }
                        pTP->PrivilegeCount = privilegeCount;

                        success = ListenerUnsafeNativeMethods.AdjustTokenPrivileges(token, false, pTP, tokenInformation.Length, IntPtr.Zero, IntPtr.Zero);
                        int error = Marshal.GetLastWin32Error();
                        if (!success || error != UnsafeNativeMethods.ERROR_SUCCESS)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
                        }
                    }
                }
                finally
                {
                    token.Close();
                }
            }
            finally
            {
                process.Close();
            }
        }

        // Do not use this method unless you understand the consequnces of lack of synchronization
        static void EditKernelObjectSecurity(SafeCloseHandle kernelObject, List<SecurityIdentifier> accounts, SecurityIdentifier account, int right, bool add)
        {
            // take the SECURITY_DESCRIPTOR from the kernelObject
            int lpnLengthNeeded;
            bool success = ListenerUnsafeNativeMethods.GetKernelObjectSecurity(kernelObject, ListenerUnsafeNativeMethods.DACL_SECURITY_INFORMATION, null, 0, out lpnLengthNeeded);
            if (!success)
            {
                int errorCode = Marshal.GetLastWin32Error();
                if (errorCode != ListenerUnsafeNativeMethods.ERROR_INSUFFICIENT_BUFFER)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(errorCode));
                }
            }
            byte[] pSecurityDescriptor = new byte[lpnLengthNeeded];
#pragma warning suppress 56523 // Microsoft, Win32Exception ctor calls Marshal.GetLastWin32Error()
            success = ListenerUnsafeNativeMethods.GetKernelObjectSecurity(kernelObject, ListenerUnsafeNativeMethods.DACL_SECURITY_INFORMATION, pSecurityDescriptor, pSecurityDescriptor.Length, out lpnLengthNeeded);
            if (!success)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception());
            }
            CommonSecurityDescriptor securityDescriptor = new CommonSecurityDescriptor(false, false, pSecurityDescriptor, 0);
            DiscretionaryAcl dacl = securityDescriptor.DiscretionaryAcl;
            // add ACEs to the SECURITY_DESCRIPTOR of the kernelObject
            if (account != null)
            {
                EditDacl(dacl, account, right, add);
            }
            else if (accounts != null)
            {
                foreach (SecurityIdentifier accountInList in accounts)
                {
                    EditDacl(dacl, accountInList, right, add);
                }
            }
            lpnLengthNeeded = securityDescriptor.BinaryLength;
            pSecurityDescriptor = new byte[lpnLengthNeeded];
            securityDescriptor.GetBinaryForm(pSecurityDescriptor, 0);
            // set the SECURITY_DESCRIPTOR on the kernelObject
#pragma warning suppress 56523 // Microsoft, Win32Exception ctor calls Marshal.GetLastWin32Error()
            success = ListenerUnsafeNativeMethods.SetKernelObjectSecurity(kernelObject, ListenerUnsafeNativeMethods.DACL_SECURITY_INFORMATION, pSecurityDescriptor);
            if (!success)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception());
            }
        }

        static void EditDacl(DiscretionaryAcl dacl, SecurityIdentifier account, int right, bool add)
        {
            if (add)
            {
                dacl.AddAccess(AccessControlType.Allow, account, right, InheritanceFlags.None, PropagationFlags.None);
            }
            else
            {
                dacl.RemoveAccess(AccessControlType.Allow, account, right, InheritanceFlags.None, PropagationFlags.None);
            }
        }

        internal static SecurityIdentifier GetWindowsServiceSid(string name)
        {
            Fx.Assert(OSEnvironmentHelper.IsVistaOrGreater, "This method can be called only on Vista or greater.");
            string accountName = string.Format(CultureInfo.InvariantCulture, WindowsServiceAccountFormat, name);

            byte[] sid = null;
            uint cbSid = 0;
            uint cchReferencedDomainName = 0;
            short peUse;
            int error = UnsafeNativeMethods.ERROR_SUCCESS;
            if (!ListenerUnsafeNativeMethods.LookupAccountName(null, accountName, sid, ref cbSid,
                null, ref cchReferencedDomainName, out peUse))
            {
                error = Marshal.GetLastWin32Error();
                if (error != ListenerUnsafeNativeMethods.ERROR_INSUFFICIENT_BUFFER)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
                }
            }

            sid = new byte[cbSid];
            StringBuilder referencedDomainName = new StringBuilder((int)cchReferencedDomainName);
            if (!ListenerUnsafeNativeMethods.LookupAccountName(null, accountName, sid, ref cbSid,
                referencedDomainName, ref cchReferencedDomainName, out peUse))
            {
                error = Marshal.GetLastWin32Error();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
            }

            return new SecurityIdentifier(sid, 0);
        }

        internal static int GetPidForService(string serviceName)
        {
            return GetStatusForService(serviceName).dwProcessId;
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        internal static SecurityIdentifier GetLogonSidForPid(int pid)
        {
            SafeCloseHandle process = OpenProcessForQuery(pid);
            try
            {
                SafeCloseHandle token = GetProcessToken(process, ListenerUnsafeNativeMethods.TOKEN_QUERY);
                try
                {
                    int length = GetTokenInformationLength(token, ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS.TokenGroups);
                    byte[] tokenInformation = new byte[length];
                    fixed (byte* pTokenInformation = tokenInformation)
                    {
                        GetTokenInformation(token, ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS.TokenGroups, tokenInformation);

                        ListenerUnsafeNativeMethods.TOKEN_GROUPS* ptg = (ListenerUnsafeNativeMethods.TOKEN_GROUPS*)pTokenInformation;
                        ListenerUnsafeNativeMethods.SID_AND_ATTRIBUTES* sids = (ListenerUnsafeNativeMethods.SID_AND_ATTRIBUTES*)(&(ptg->Groups));
                        for (int i = 0; i < ptg->GroupCount; i++)
                        {
                            if ((sids[i].Attributes & ListenerUnsafeNativeMethods.SidAttribute.SE_GROUP_LOGON_ID) == ListenerUnsafeNativeMethods.SidAttribute.SE_GROUP_LOGON_ID)
                            {
                                return new SecurityIdentifier(sids[i].Sid);
                            }
                        }
                    }
                    return new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
                }
                finally
                {
                    token.Close();
                }
            }
            finally
            {
                process.Close();
            }
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        internal static SecurityIdentifier GetUserSidForPid(int pid)
        {
            SafeCloseHandle process = OpenProcessForQuery(pid);
            try
            {
                SafeCloseHandle token = GetProcessToken(process, ListenerUnsafeNativeMethods.TOKEN_QUERY);
                try
                {
                    int length = GetTokenInformationLength(token, ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS.TokenUser);
                    byte[] tokenInformation = new byte[length];
                    fixed (byte* pTokenInformation = tokenInformation)
                    {
                        GetTokenInformation(token, ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS.TokenUser, tokenInformation);

                        ListenerUnsafeNativeMethods.TOKEN_USER* ptg = (ListenerUnsafeNativeMethods.TOKEN_USER*)pTokenInformation;
                        ListenerUnsafeNativeMethods.SID_AND_ATTRIBUTES* sids = (ListenerUnsafeNativeMethods.SID_AND_ATTRIBUTES*)(&(ptg->User));
                        return new SecurityIdentifier(sids->Sid);
                    }
                }
                finally
                {
                    token.Close();
                }
            }
            finally
            {
                process.Close();
            }
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        static ListenerUnsafeNativeMethods.SERVICE_STATUS_PROCESS GetStatusForService(string serviceName)
        {
            SafeServiceHandle scManager = OpenSCManager();
            try
            {
                SafeServiceHandle service = OpenService(scManager, serviceName, ListenerUnsafeNativeMethods.SERVICE_QUERY_STATUS);
                try
                {
                    int lpnLengthNeeded;
                    bool success = ListenerUnsafeNativeMethods.QueryServiceStatusEx(service, ListenerUnsafeNativeMethods.SC_STATUS_PROCESS_INFO, null, 0, out lpnLengthNeeded);
                    if (!success)
                    {
                        int errorCode = Marshal.GetLastWin32Error();
                        if (errorCode != ListenerUnsafeNativeMethods.ERROR_INSUFFICIENT_BUFFER)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(errorCode));
                        }
                    }
                    byte[] serviceStatusProcess = new byte[lpnLengthNeeded];
#pragma warning suppress 56523 // Microsoft, Win32Exception ctor calls Marshal.GetLastWin32Error()
                    success = ListenerUnsafeNativeMethods.QueryServiceStatusEx(service, ListenerUnsafeNativeMethods.SC_STATUS_PROCESS_INFO, serviceStatusProcess, serviceStatusProcess.Length, out lpnLengthNeeded);
                    if (!success)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception());
                    }
                    fixed (byte* pServiceStatusProcess = serviceStatusProcess)
                    {
                        return (ListenerUnsafeNativeMethods.SERVICE_STATUS_PROCESS)Marshal.PtrToStructure((IntPtr)pServiceStatusProcess, typeof(ListenerUnsafeNativeMethods.SERVICE_STATUS_PROCESS));
                    }
                }
                finally
                {
                    service.Close();
                }
            }
            finally
            {
                scManager.Close();
            }
        }
    }
}
