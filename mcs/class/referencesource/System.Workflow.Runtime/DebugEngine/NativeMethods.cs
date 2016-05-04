// Copyright (c) Microsoft Corp., 2004. All rights reserved.
#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;

#endregion

namespace System.Workflow.Runtime.DebugEngine
{
    internal static class NativeMethods
    {
        public const int STANDARD_RIGHTS_REQUIRED = (0x000F0000);
        public const int TOKEN_ASSIGN_PRIMARY = (0x0001);
        public const int TOKEN_DUPLICATE = (0x0002);
        public const int TOKEN_IMPERSONATE = (0x0004);
        public const int TOKEN_QUERY = (0x0008);
        public const int TOKEN_QUERY_SOURCE = (0x0010);
        public const int TOKEN_ADJUST_PRIVILEGES = (0x0020);
        public const int TOKEN_ADJUST_GROUPS = (0x0040);
        public const int TOKEN_ADJUST_DEFAULT = (0x0080);
        public const int TOKEN_ADJUST_SESSIONID = (0x0100);

        public const int TOKEN_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED |
                          TOKEN_ASSIGN_PRIMARY |
                          TOKEN_DUPLICATE |
                          TOKEN_IMPERSONATE |
                          TOKEN_QUERY |
                          TOKEN_QUERY_SOURCE |
                          TOKEN_ADJUST_PRIVILEGES |
                          TOKEN_ADJUST_GROUPS |
                          TOKEN_ADJUST_DEFAULT);

        [Flags]
        public enum SECURITY_INFORMATION : uint
        {
            OWNER_SECURITY_INFORMATION = 0x00000001,
            GROUP_SECURITY_INFORMATION = 0x00000002,
            DACL_SECURITY_INFORMATION = 0x00000004,
            SACL_SECURITY_INFORMATION = 0x00000008,
            UNPROTECTED_SACL_SECURITY_INFORMATION = 0x10000000,
            UNPROTECTED_DACL_SECURITY_INFORMATION = 0x20000000,
            PROTECTED_SACL_SECURITY_INFORMATION = 0x40000000,
            PROTECTED_DACL_SECURITY_INFORMATION = 0x80000000
        }

        [Flags]
        public enum RpcAuthnLevel
        {
            Default = 0,
            None = 1,
            Connect = 2,
            Call = 3,
            Pkt = 4,
            PktIntegrity = 5,
            PktPrivacy = 6
        }

        public enum EoAuthnCap
        {
            None = 0x00,
            MutualAuth = 0x01,
            StaticCloaking = 0x20,
            DynamicCloaking = 0x40,
            AnyAuthority = 0x80,
            MakeFullSIC = 0x100,
            Default = 0x800,
            SecureRefs = 0x02,
            AccessControl = 0x04,
            AppID = 0x08,
            Dynamic = 0x10,
            RequireFullSIC = 0x200,
            AutoImpersonate = 0x400,
            NoCustomMarshal = 0x2000,
            DisableAAA = 0x1000
        }

        public enum RpcImpLevel
        {
            Default = 0,
            Anonymous = 1,
            Identify = 2,
            Impersonate = 3,
            Delegate = 4
        }

        [DllImport("kernel32.dll", SetLastError = false)]
        public static extern int GetCurrentThreadId();

        [DllImport("kernel32.dll", SetLastError = false)]
        public static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool RevertToSelf();

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool OpenProcessToken(IntPtr ProcessHandle, UInt32 DesiredAccess, out IntPtr TokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool GetKernelObjectSecurity(IntPtr Handle, SECURITY_INFORMATION RequestedInformation, IntPtr pSecurityDescriptor, UInt32 nLength, out UInt32 lpnLengthNeeded);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool SetKernelObjectSecurity(IntPtr Handle, SECURITY_INFORMATION SecurityInformation, IntPtr SecurityDescriptor);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

    }

    internal static class Guids
    {
        internal const string CLSID_WDEProgramPublisher = "B6C0E598-314D-4b63-8C5C-4014F2A1B737";
        public const string IID_IWDEProgramNode = "e5e93adb-a6fe-435e-8640-31ae310d812f";
        public const string IID_IWDEProgramPublisher = "2BE74789-F70B-42a3-80CA-E91743385844";
    }
}
