// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// This file reiimplements CoreFX source file 'src/Common/src/Interop/Unix/System.Native/Interop.LingerOptions.cs'

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal static partial class Interop
{
	internal static partial class Sys
	{
		internal struct LingerOption
		{
			public int OnOff;   // Non-zero to enable linger
			public int Seconds; // Number of seconds to linger for
		}

		internal static unsafe Error GetLingerOption (SafeHandle socket, LingerOption* option)
		{
			return (Error)GetLingerOption_internal (socket.DangerousGetHandle (), option);
		}

		internal static unsafe Error SetLingerOption (SafeHandle socket, LingerOption* option)
		{
			return (Error)SetLingerOption_internal (socket.DangerousGetHandle (), option);
		}

		internal static unsafe Error SetLingerOption (IntPtr socket, LingerOption* option)
		{
			return (Error)SetLingerOption_internal (socket, option);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern unsafe int GetLingerOption_internal (IntPtr socket, LingerOption* option);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern unsafe int SetLingerOption_internal (IntPtr socket, LingerOption* option);
	}
}
