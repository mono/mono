// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace System
{
    partial struct DateTime
    {
        public static DateTime UtcNow 
        {
            get 
            {
                long ticks = GetSystemTimeAsFileTime();
                return new DateTime(((UInt64)(ticks + FileTimeOffset)) | KindUtc);
            }
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern long GetSystemTimeAsFileTime();

        internal Int64 ToBinaryRaw() => (Int64)_dateData;
    }
}