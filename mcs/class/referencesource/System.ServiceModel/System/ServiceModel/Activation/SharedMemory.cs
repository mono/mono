//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.Activation
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Security;
    using System.Security.Principal;
    using System.Security.Permissions;
    using System.ServiceModel.Channels;
    using System.Threading;
    using System.ServiceModel;
    using System.ComponentModel;

    unsafe class SharedMemory : IDisposable
    {
        SafeFileMappingHandle fileMapping;

        SharedMemory(SafeFileMappingHandle fileMapping)
        {
            this.fileMapping = fileMapping;
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        public static unsafe SharedMemory Create(string name, Guid content, List<SecurityIdentifier> allowedSids)
        {
            int errorCode = UnsafeNativeMethods.ERROR_SUCCESS;
            byte[] binarySecurityDescriptor = SecurityDescriptorHelper.FromSecurityIdentifiers(allowedSids, UnsafeNativeMethods.GENERIC_READ);
            SafeFileMappingHandle fileMapping;
            UnsafeNativeMethods.SECURITY_ATTRIBUTES securityAttributes = new UnsafeNativeMethods.SECURITY_ATTRIBUTES();
            fixed (byte* pinnedSecurityDescriptor = binarySecurityDescriptor)
            {
                securityAttributes.lpSecurityDescriptor = (IntPtr)pinnedSecurityDescriptor;
                fileMapping = UnsafeNativeMethods.CreateFileMapping((IntPtr)(-1), securityAttributes, UnsafeNativeMethods.PAGE_READWRITE, 0, sizeof(SharedMemoryContents), name);
                errorCode = Marshal.GetLastWin32Error();
            }

            if (fileMapping.IsInvalid)
            {
                fileMapping.SetHandleAsInvalid();
                fileMapping.Close();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(errorCode));
            }

            SharedMemory sharedMemory = new SharedMemory(fileMapping);
            SafeViewOfFileHandle view;

            // Ignore return value.
            GetView(fileMapping, true, out view);

            try
            {
                SharedMemoryContents* contents = (SharedMemoryContents*)view.DangerousGetHandle();
                contents->pipeGuid = content;
                Thread.MemoryBarrier();
                contents->isInitialized = true;
                return sharedMemory;
            }
            finally
            {
                view.Close();
            }
        }

        public void Dispose()
        {
            if (fileMapping != null)
            {
                fileMapping.Close();
                fileMapping = null;
            }
        }

        static bool GetView(SafeFileMappingHandle fileMapping, bool writable, out SafeViewOfFileHandle handle)
        {
            handle = UnsafeNativeMethods.MapViewOfFile(fileMapping, writable ? UnsafeNativeMethods.FILE_MAP_WRITE : UnsafeNativeMethods.FILE_MAP_READ, 0, 0, (IntPtr)sizeof(SharedMemoryContents));
            int errorCode = Marshal.GetLastWin32Error();
            if (!handle.IsInvalid)
            {
                return true;
            }
            else
            {
                handle.SetHandleAsInvalid();
                fileMapping.Close();

                // Only return false when it's reading time.
                if (!writable && errorCode == UnsafeNativeMethods.ERROR_FILE_NOT_FOUND)
                {
                    return false;
                }

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(errorCode));
            }
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        public static string Read(string name)
        {
            string content;
            if (Read(name, out content))
            {
                return content;
            }

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(UnsafeNativeMethods.ERROR_FILE_NOT_FOUND));
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        [ResourceConsumption(ResourceScope.Machine)]
        public static bool Read(string name, out string content)
        {
            content = null;

            SafeFileMappingHandle fileMapping = UnsafeNativeMethods.OpenFileMapping(UnsafeNativeMethods.FILE_MAP_READ, false, ListenerConstants.GlobalPrefix + name);
            int errorCode = Marshal.GetLastWin32Error();
            if (fileMapping.IsInvalid)
            {
                fileMapping.SetHandleAsInvalid();
                fileMapping.Close();
                if (errorCode == UnsafeNativeMethods.ERROR_FILE_NOT_FOUND)
                {
                    return false;
                }

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(errorCode));
            }

            try
            {
                SafeViewOfFileHandle view;
                if (!GetView(fileMapping, false, out view))
                {
                    return false;
                }

                try
                {
                    SharedMemoryContents* contents = (SharedMemoryContents*)view.DangerousGetHandle();
                    content = contents->isInitialized ? contents->pipeGuid.ToString() : null;
                    return true;
                }
                finally
                {
                    view.Close();
                }
            }
            finally
            {
                fileMapping.Close();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SharedMemoryContents
        {
            public bool isInitialized;
            public Guid pipeGuid;
        }
    }
}
