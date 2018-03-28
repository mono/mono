// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Classes:  MemoryMappedFileAccess
**           MemoryMappedFileOptions
**
** Purpose:  Enums for managed MemoryMappedFiles.
**
** Date:  February 7, 2007 
**
===========================================================*/

using System;

namespace System.IO.MemoryMappedFiles {

    // This enum maps to both the PAGE_XXX and FILE_MAP_XXX native macro definitions.
    // It is used in places that check the page access of the memory mapped file. ACL
    // access is controlled by MemoryMappedFileRights.
    [Serializable]
    public enum MemoryMappedFileAccess {
        ReadWrite = 0,
        Read,
        Write,   // Write is valid only when creating views and not when creating MemoryMappedFiles   
        CopyOnWrite,
        ReadExecute,
        ReadWriteExecute,
    }

    [Serializable, Flags]
    public enum MemoryMappedFileOptions {
        None = 0,
        DelayAllocatePages = 0x4000000
    }

}
