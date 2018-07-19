// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:   MemoryMappedFileSecurity
**
** Purpose: Managed ACL wrapper for MemoryMappedFiles.
**
** Date:  February 7, 2007
**
===========================================================*/

using System;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Security.Principal;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace System.IO.MemoryMappedFiles
{
    [Flags]
    public enum MemoryMappedFileRights
    {
        // These correspond to win32 FILE_MAP_XXX constants

        // No None field - An ACE with the value 0 cannot grant nor deny.
        CopyOnWrite                  = 0x000001,
        Write                        = 0x000002,
        Read                         = 0x000004,
        Execute                      = 0x000008,

        Delete                       = 0x010000,
        ReadPermissions              = 0x020000,
        ChangePermissions            = 0x040000,
        TakeOwnership                = 0x080000,
        //Synchronize                = Not supported by memory mapped files

        ReadWrite                    = Read | Write,
        ReadExecute                  = Read | Execute,
        ReadWriteExecute             = Read | Write | Execute,

        FullControl                  = CopyOnWrite | Read | Write | Execute | Delete | 
                                       ReadPermissions | ChangePermissions | TakeOwnership,

        AccessSystemSecurity         = 0x01000000, // Allow changes to SACL
    }

    public class MemoryMappedFileSecurity : ObjectSecurity<MemoryMappedFileRights> 
    {
        public MemoryMappedFileSecurity()
            : base(false, ResourceType.KernelObject)
        { }

        [System.Security.SecuritySafeCritical]
        internal MemoryMappedFileSecurity(SafeMemoryMappedFileHandle safeHandle, AccessControlSections includeSections )
            : base(false, ResourceType.KernelObject, safeHandle, includeSections)
        { }

        [System.Security.SecuritySafeCritical]
        internal void PersistHandle(SafeHandle handle) {
            Persist(handle);
        }
    }
}
