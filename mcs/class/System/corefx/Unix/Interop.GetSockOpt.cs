// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// This file reiimplements CoreFX source file 'src/Common/src/Interop/Unix/System.Native/Interop.GetSockOpt.cs'

using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal static partial class Interop
{
	internal static partial class Sys
	{
		internal static unsafe Error GetSockOpt (SafeHandle socket, SocketOptionLevel optionLevel, SocketOptionName optionName, byte* optionValue, int* optionLen)
		{
			return (Error)GetSockOpt_internal (socket.DangerousGetHandle (), (int)optionLevel, (int)optionName, optionValue, optionLen);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern unsafe int GetSockOpt_internal (IntPtr socket, int optionLevel, int optionName, byte* optionValue, int* optionLen);
	}
}
