// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// This file reiimplements CoreFX source file 'src/Common/src/Interop/Unix/System.Native/Interop.MulticastOption.cs'

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal static partial class Interop
{
	internal static partial class Sys
	{
		internal enum MulticastOption : int
		{
			MULTICAST_ADD = 0,
			MULTICAST_DROP = 1,
			MULTICAST_IF = 2
		}

		internal struct IPv4MulticastOption
		{
			public uint MulticastAddress;
			public uint LocalAddress;
			public int InterfaceIndex;
			private int _padding;
		}

		internal struct IPv6MulticastOption
		{
			public IPAddress Address;
			public int InterfaceIndex;
			private int _padding;
		}

		internal static unsafe Error GetIPv4MulticastOption (SafeHandle socket, MulticastOption multicastOption, IPv4MulticastOption* option)
		{
			return (Error)GetIPv4MulticastOption_internal (socket.DangerousGetHandle (), (int)multicastOption, option);
		}

		internal static unsafe Error SetIPv4MulticastOption (SafeHandle socket, MulticastOption multicastOption, IPv4MulticastOption* option)
		{
			return (Error)SetIPv4MulticastOption_internal (socket.DangerousGetHandle (), (int)multicastOption, option);
		}

		internal static unsafe Error GetIPv6MulticastOption (SafeHandle socket, MulticastOption multicastOption, IPv6MulticastOption* option)
		{
			return (Error)GetIPv6MulticastOption_internal (socket.DangerousGetHandle (), (int)multicastOption, option);
		}

		internal static unsafe Error SetIPv6MulticastOption (SafeHandle socket, MulticastOption multicastOption, IPv6MulticastOption* option)
		{
			return (Error)SetIPv6MulticastOption_internal (socket.DangerousGetHandle (), (int)multicastOption, option);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern unsafe int GetIPv4MulticastOption_internal (IntPtr socket, int multicastOption, IPv4MulticastOption* option);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern unsafe int SetIPv4MulticastOption_internal (IntPtr socket, int multicastOption, IPv4MulticastOption* option);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern unsafe int GetIPv6MulticastOption_internal (IntPtr socket, int multicastOption, IPv6MulticastOption* option);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern unsafe int SetIPv6MulticastOption_internal (IntPtr socket, int multicastOption, IPv6MulticastOption* option);
	}
}
