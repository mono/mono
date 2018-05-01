// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Win32.SafeHandles;
using System.Security;

namespace System.IO.MemoryMappedFiles
{
    public partial class MemoryMappedFile
    {
        private static unsafe SafeMemoryMappedFileHandle CreateCore(
            FileStream fileStream, string mapName, 
            HandleInheritability inheritability, MemoryMappedFileAccess access, 
            MemoryMappedFileOptions options, long capacity)
        {
            throw new PlatformNotSupportedException ();
        }


        private static SafeMemoryMappedFileHandle CreateOrOpenCore(
            string mapName, 
            HandleInheritability inheritability, MemoryMappedFileAccess access,
            MemoryMappedFileOptions options, long capacity)
        {
            throw new PlatformNotSupportedException ();
        }

        private static SafeMemoryMappedFileHandle OpenCore(
            string mapName, HandleInheritability inheritability, MemoryMappedFileAccess access, bool createOrOpen)
        {
            throw new PlatformNotSupportedException ();
        }

        private static SafeMemoryMappedFileHandle OpenCore(
            string mapName, HandleInheritability inheritability, MemoryMappedFileRights rights, bool createOrOpen)
        {
            throw new PlatformNotSupportedException ();
        }

    }
}
