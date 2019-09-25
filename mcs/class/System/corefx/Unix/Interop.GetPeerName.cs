// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// This file reiimplements CoreFX source file 'src/Common/src/Interop/Unix/System.Native/Interop.GetPeerName.cs'

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal static partial class Interop
{
	internal static partial class Sys
	{
		internal static unsafe Error GetPeerName (SafeHandle socket, byte* socketAddress, int* socketAddressLen)
		{
			return (Error)GetPeerName_internal (socket.DangerousGetHandle (), socketAddress, socketAddressLen);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern unsafe int GetPeerName_internal (IntPtr socket, byte* socketAddress, int* socketAddressLen);
	}
}
