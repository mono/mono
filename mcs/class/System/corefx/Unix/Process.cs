// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Diagnostics
{
    partial class Process : IDisposable
    {
        // Process.UnknownLinux.cs for some reason doesn't have it
        private static string GetPathToOpenFile () => throw new PlatformNotSupportedException();
    }
}
