// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// This file reiimplements CoreFX source file 'src/Common/src/Interop/Unix/System.Native/Interop.SetSendTimeout.cs'

internal static partial class Interop
{
	internal static partial class Sys
	{
		internal static unsafe Error SetSendTimeout (SafeHandle socket, int millisecondsTimeout)
		{
			return (Error)SetSendTimeout_internal (socket.DangerousGetHandle (), millisecondsTimeout);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern unsafe int SetSendTimeout_internal (IntPtr socket, int millisecondsTimeout);
	}
}
