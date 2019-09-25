// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// This file reiimplements CoreFX source file 'src/Common/src/Interop/Unix/System.Native/Interop.Shutdown.cs'

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System;
using System.Net.Sockets;

internal static partial class Interop
{
	internal static partial class Sys
	{
		internal static unsafe Error Shutdown (SafeHandle socket, SocketShutdown how)
		{
			return (Error)Shutdown_internal (socket.DangerousGetHandle (), (int)how);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern unsafe int Shutdown_internal (IntPtr handle, int how);
	}
}
