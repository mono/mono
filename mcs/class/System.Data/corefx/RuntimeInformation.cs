// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Reflection;

namespace System.Runtime.InteropServices
{
    internal enum OSPlatform
    {
        OSX,
        Linux,
        Windows
    }

    internal static class RuntimeInformation
    {
        internal static bool IsOSPlatform(OSPlatform osPlatform) {
            switch (Environment.OSVersion.Platform) {
                case PlatformID.Win32NT:
                    return osPlatform == OSPlatform.Windows;
                case PlatformID.Unix:
                    if (File.Exists ("/usr/lib/libc.dylib"))
                        return osPlatform == OSPlatform.OSX;
                    return osPlatform == OSPlatform.Linux;
                default:
                    return false;
            }
        }
    }
}