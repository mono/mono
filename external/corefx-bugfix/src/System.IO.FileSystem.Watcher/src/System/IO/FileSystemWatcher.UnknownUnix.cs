// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
#if MONO && !MOBILE

using System;
using System.IO;
namespace System.IO.CoreFX
#else

namespace System.IO
#endif
{
    public partial class FileSystemWatcher
    {
        /// <summary>Called when FileSystemWatcher is finalized.</summary>
        private void FinalizeDispose()
        {
        }

        private void StartRaisingEvents()
        {
            throw new PlatformNotSupportedException();
        }

        private void StopRaisingEvents()
        {
            throw new PlatformNotSupportedException();
        }
    }
}
