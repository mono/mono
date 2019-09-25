// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// This file reiimplements CoreFX source file 'src/Common/src/Interop/Unix/System.Native/Interop.GetSocketErrorOption.cs

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal static partial class Interop
{
	internal static partial class Sys
	{
		internal static unsafe Error GetSocketErrorOption (SafeHandle socket, Error *socketError)
		{
			return (Error)GetSocketErrorOption_internal (socket.DangerousGetHandle (), (int*)socketError);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern unsafe int GetSocketErrorOption_internal (IntPtr socket, int* socketError);
	}
}
