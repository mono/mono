// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// This file reiimplements CoreFX source file 'src/Common/src/Interop/Unix/System.Native/Interop.GetBytesAvailable.cs'

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal static partial class Interop
{
	internal static partial class Sys
	{
		internal static unsafe Error GetBytesAvailable (SafeHandle socket, int* available)
		{
			return (Error)GetBytesAvailable_internal (socket.DangerousGetHandle (), available);
		}

		internal static unsafe Error GetAtOutOfBandMark (SafeHandle socket, int* atMark)
		{
			return (Error)GetAtOutOfBandMark_internal (socket.DangerousGetHandle (), atMark);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern unsafe int GetBytesAvailable_internal (IntPtr socket, int* available);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern unsafe int GetAtOutOfBandMark_internal (IntPtr socket, int* atMark);
	}
}
