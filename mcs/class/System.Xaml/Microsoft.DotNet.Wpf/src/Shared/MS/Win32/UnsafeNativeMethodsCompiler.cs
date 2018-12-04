//------------------------------------------------------------------------------
// <copyright file="UnsafeNativeMethodsCompiler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Text;
using System.ComponentModel;
using System.Security;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace MS.Win32.Compile
{

    //
    // Keep unsafe native methods used by Compiler related classes.
    // It can be shared by PresentationBuildTasks and PresentationFramework
    //
#if !PBTCOMPILER
    [MS.Internal.PresentationCore.FriendAccessAllowed] // Used by both PBT and PresentationFramework
#endif
    internal static partial class UnsafeNativeMethods {

        //------------------------------------------------------
        //
        //  Internal Methods
        //
        //------------------------------------------------------

        #region internal Methods

#if !PBTCOMPILER
        [DllImport(ExternDll.Shfolder, CharSet = CharSet.Auto, BestFitMapping = false)]
        public static extern int SHGetFolderPath(IntPtr hwndOwner, int nFolder, IntPtr hToken, int dwFlags, StringBuilder lpszPath);
#endif

        /// <SecurityNote>
        ///     Critical: This code calls into unmanaged code
        /// </SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport("urlmon.dll", CharSet = CharSet.Unicode)]
        internal static extern int FindMimeFromData(
                        IBindCtx pBC,                   // bind context - can be NULL
                        string wszUrl,                  // url - can be null
                        IntPtr Buffer,                  // buffer with data to sniff -
                                                        // can be null (pwzUrl must be valid)
                        int cbSize,                     // size of buffer
                        string wzMimeProposed,          // proposed mime if - can be null
                        int dwMimeFlags,                // will be determined
                        out string wzMimeOut,           // the suggested mime
                        int dwReserved);

       #endregion
    }
}

