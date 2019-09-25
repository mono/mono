// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// This file reiimplements CoreFX source file 'src/Common/src/Interop/Unix/System.Native/Interop.Connect.cs'

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System;

internal static partial class Interop
{
	internal static partial class Sys
	{
		internal static unsafe Error Connect (SafeHandle socket, byte* socketAddress, int socketAddressLen)
		{
			return (Error)Connect_internal (socket.DangerousGetHandle (), socketAddress, socketAddressLen);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern unsafe int Connect_internal (IntPtr handle, byte* socketAddress, int socketAddressLen);
	}
}
