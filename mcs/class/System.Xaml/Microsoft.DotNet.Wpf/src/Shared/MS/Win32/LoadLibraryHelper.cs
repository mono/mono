//---------------------------------------------------------------------------
//
// <copyright file="LoadLibraryHelper.cs" company="Microsoft">
//    Copyright (C) Microsoft Corporation.  All rights reserved.
// </copyright>
//
//
// Description: Helper class for calling LoadLibrary using SEARCH_SYSTEM32 when possible
//
//---------------------------------------------------------------------------

namespace MS.Win32
{
    using System;
    using System.Security;
    using System.Security.Permissions;
    using System.Runtime.InteropServices;



    internal static class LoadLibraryHelper
    {
        /// <summary>
        /// Identifies whether functionality introduced by KB2533623 is 
        /// available.
        /// </summary>
        /// <remarks>
        /// KB2533623 introduced kernel32!AddDllDirectoryName. We look for this 
        /// method to determine the result of this method.
        /// </remarks>
        /// <SecurityNote>
        ///     Critical: Calls into P/Invoke methods
        ///     Safe: Returns a safe boolean to the caller
        /// </SecurityNote>
        [SecuritySafeCritical]
        private static bool IsKnowledgeBase2533623OrGreater()
        {
            const string AddDllDirectoryName = nameof(AddDllDirectoryName);

            bool isKnowledgeBase2533623OrGreater = false;

            // We don't throw if one of these Win32 calls fail - we play it safe 
            // and return false
            var hModule = IntPtr.Zero;
            if (UnsafeNativeMethods.GetModuleHandleEx(
                UnsafeNativeMethods.GetModuleHandleFlags.None,
                ExternDll.Kernel32,
                out hModule) && 
                hModule != IntPtr.Zero)
            {
                try
                {
                    isKnowledgeBase2533623OrGreater =
                         UnsafeNativeMethods.GetProcAddressNoThrow(new HandleRef(null, hModule), AddDllDirectoryName) != IntPtr.Zero;
                }
                finally
                {
                    UnsafeNativeMethods.FreeLibrary(hModule);
                }
            }

            return isKnowledgeBase2533623OrGreater;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lpFileName"></param>
        /// <param name="hFile"></param>
        /// <param name="dwFlags"></param>
        /// <returns></returns>
        /// <remarks>
        /// The following <see cref="UnsafeNativeMethods.LoadLibraryFlags"/> require KB2532445 to be installed. The 
        /// presence of  KB2533623 can be tested by looking for AddDllDirectories export in kernel32.dll
        /// 
        /// <see cref="UnsafeNativeMethods.LoadLibraryFlags.LOAD_LIBRARY_SEARCH_APPLICATION_DIR"/>
        /// <see cref="UnsafeNativeMethods.LoadLibraryFlags.LOAD_LIBRARY_SEARCH_DEFAULT_DIRS"/>
        /// <see cref="UnsafeNativeMethods.LoadLibraryFlags.LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR"/>
        /// <see cref="UnsafeNativeMethods.LoadLibraryFlags.LOAD_LIBRARY_SEARCH_SYSTEM32"/>
        /// <see cref="UnsafeNativeMethods.LoadLibraryFlags.LOAD_LIBRARY_SEARCH_USER_DIRS"/>
        /// 
        /// The folowing flags require KB2532445. We do not provide any support for safely using this flag, and 
        /// leave it up to a future change to this implementation to add the right checks.
        /// </remarks>
        /// <SecurityNote>
        ///     Critical: 
        ///         Calls into Critical P/Invoke methods
        ///         Returns native module handle
        /// </SecurityNote>
        [SecurityCritical]
        internal static IntPtr SecureLoadLibraryEx(string lpFileName, IntPtr hFile, UnsafeNativeMethods.LoadLibraryFlags dwFlags)
        {
            if (!IsKnowledgeBase2533623OrGreater())
            {
                // Edit out the unsupported flags
                if ((dwFlags & 
                    UnsafeNativeMethods.LoadLibraryFlags.LOAD_LIBRARY_SEARCH_APPLICATION_DIR &
                    UnsafeNativeMethods.LoadLibraryFlags.LOAD_LIBRARY_SEARCH_DEFAULT_DIRS &
                    UnsafeNativeMethods.LoadLibraryFlags.LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR &
                    UnsafeNativeMethods.LoadLibraryFlags.LOAD_LIBRARY_SEARCH_SYSTEM32 &
                    UnsafeNativeMethods.LoadLibraryFlags.LOAD_LIBRARY_SEARCH_USER_DIRS) != UnsafeNativeMethods.LoadLibraryFlags.None)
                {
                    dwFlags &= ~(
                        UnsafeNativeMethods.LoadLibraryFlags.LOAD_LIBRARY_SEARCH_APPLICATION_DIR |
                        UnsafeNativeMethods.LoadLibraryFlags.LOAD_LIBRARY_SEARCH_DEFAULT_DIRS |
                        UnsafeNativeMethods.LoadLibraryFlags.LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR |
                        UnsafeNativeMethods.LoadLibraryFlags.LOAD_LIBRARY_SEARCH_SYSTEM32 |
                        UnsafeNativeMethods.LoadLibraryFlags.LOAD_LIBRARY_SEARCH_USER_DIRS);
                }
            }

#pragma warning disable CS0618 // Type or member is obsolete
            return UnsafeNativeMethods.LoadLibraryEx(lpFileName, hFile, dwFlags);
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
