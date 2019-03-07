// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if !MOBILE
namespace System.IO.CoreFX
#else
namespace System.IO
#endif
{
    // Add DefaultEventAdttribute for NS2.1 support
    [System.ComponentModel.DefaultEventAttribute("Changed")]
    public partial class FileSystemWatcher
    {
    }
}
